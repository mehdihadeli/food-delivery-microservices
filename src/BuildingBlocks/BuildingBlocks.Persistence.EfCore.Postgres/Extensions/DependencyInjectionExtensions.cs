using System.Reflection;
using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Abstractions.Persistence.EfCore;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollectionExtensions;
using BuildingBlocks.Core.Persistence.EfCore;
using BuildingBlocks.Core.Persistence.EfCore.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Persistence.EfCore.Postgres;

public static class DependencyInjectionExtensions
{
    public static IHostApplicationBuilder AddPostgresDbContext<TDbContext>(
        this IHostApplicationBuilder builder,
        string? connectionStringName,
        Assembly? migrationAssembly = null,
        Action<IHostApplicationBuilder>? action = null,
        Action<DbContextOptionsBuilder>? dbContextBuilder = null,
        Action<PostgresOptions>? configurator = null,
        params Assembly[] assembliesToScan
    )
        where TDbContext : DbContext, IDbFacadeResolver, IDomainEventContext
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        // Add an option to the dependency injection
        builder.Services.AddValidationOptions(configurator: configurator);

        var postgresOptions = builder.Configuration.BindOptions<PostgresOptions>();

        // - EnvironmentVariablesConfigurationProvider is injected by aspire and use to read configuration values from environment variables with `ConnectionStrings:pg-catalogsdb` key on configuration.
        // The configuration provider handles these conversions automatically, and `__ (double underscore)` becomes `:` for nested sections,
        // so environment configuration reads its data from the ` ConnectionStrings__pg-catalogsdb ` environment. all envs are available in `Environment.GetEnvironmentVariables()`.
        // - For setting none sensitive configuration, we can use Aspire named configuration `Aspire:Npgsql:EntityFrameworkCore:PostgreSQL:DisableHealthChecks` which is of type ConfigurationProvider and should be set in appsetting.json

        // https://learn.microsoft.com/en-us/dotnet/aspire/database/postgresql-entity-framework-integration?tabs=dotnet-cli#use-a-connection-string
        // first read from aspire injected ConnectionString then read from config
        var connectionString =
            !string.IsNullOrWhiteSpace(connectionStringName)
            && !string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString(connectionStringName))
                ? builder.Configuration.GetConnectionString(connectionStringName)
                : postgresOptions.ConnectionString
                    ?? throw new InvalidOperationException(
                        $"Postgres connection string '{connectionStringName}' or `postgresOptions.ConnectionString` not found."
                    );

        builder.Services.AddScoped<IConnectionFactory>(sp => new NpgsqlConnectionFactory(connectionString));

        builder.Services.AddDbContext<TDbContext>(
            (sp, options) =>
            {
                // https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-9.0/breaking-changes#pending-model-changes
                // https://github.com/dotnet/efcore/issues/35158
                options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));

                options
                    .UseNpgsql(
                        connectionString,
                        sqlOptions =>
                        {
                            var name =
                                migrationAssembly?.GetName().Name
                                ?? postgresOptions.MigrationAssembly
                                ?? typeof(TDbContext).Assembly.GetName().Name;

                            sqlOptions.MigrationsAssembly(name);
                            sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                        }
                    )
                    // https://github.com/efcore/EFCore.NamingConventions
                    .UseSnakeCaseNamingConvention();

                // ref: https://andrewlock.net/series/using-strongly-typed-entity-ids-to-avoid-primitive-obsession/
                options.ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector<long>>();

                options.AddInterceptors(
                    new AuditInterceptor(),
                    new SoftDeleteInterceptor(),
                    new ConcurrencyInterceptor(),
                    new AggregatesDomainEventsStorageInterceptor(
                        sp.GetRequiredService<IAggregatesDomainEventsRequestStorage>()
                    )
                );

                dbContextBuilder?.Invoke(options);
            }
        );

        // https://learn.microsoft.com/en-us/dotnet/aspire/database/postgresql-entity-framework-integration?tabs=dotnet-cli#enrich-an-npgsql-database-context
        // For config health check and instrumentation for postgres dbcontext
        builder.EnrichNpgsqlDbContext<TDbContext>();

        action?.Invoke(builder);

        builder.Services.AddScoped<IDbFacadeResolver>(provider => provider.GetService<TDbContext>()!);
        builder.Services.AddScoped<IDomainEventContext>(provider => provider.GetService<TDbContext>()!);

        builder.AddPostgresRepositories(assembliesToScan);
        builder.AddPostgresUnitOfWork(assembliesToScan);

        return builder;
    }

    private static IHostApplicationBuilder AddPostgresRepositories(
        this IHostApplicationBuilder builder,
        params Assembly[] assembliesToScan
    )
    {
        var scanAssemblies = assembliesToScan.Length != 0 ? assembliesToScan : [Assembly.GetCallingAssembly()];
        builder.Services.Scan(scan =>
            scan.FromAssemblies(scanAssemblies)
                .AddClasses(classes => classes.AssignableTo(typeof(IRepository<,>)), false)
                .AsImplementedInterfaces()
                .AsSelf()
                .WithTransientLifetime()
        );

        return builder;
    }

    private static IHostApplicationBuilder AddPostgresUnitOfWork(
        this IHostApplicationBuilder builder,
        params Assembly[] assembliesToScan
    )
    {
        var scanAssemblies = assembliesToScan.Length != 0 ? assembliesToScan : [Assembly.GetCallingAssembly()];
        builder.Services.Scan(scan =>
            scan.FromAssemblies(scanAssemblies)
                .AddClasses(classes => classes.AssignableTo(typeof(IEfUnitOfWork<>)), false)
                .AsImplementedInterfaces()
                .AsSelf()
                .WithTransientLifetime()
        );

        return builder;
    }
}
