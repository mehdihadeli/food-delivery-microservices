using System.Reflection;
using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Abstractions.Persistence.EfCore;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollectionExtensions;
using BuildingBlocks.Core.Persistence.EfCore;
using BuildingBlocks.Core.Persistence.EfCore.Interceptors;
using Core.Persistence.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Persistence.EfCore.Postgres;

public static class DependencyInjectionExtensions
{
    public static IHostApplicationBuilder AddPostgresDbContext<TDbContext>(
        this IHostApplicationBuilder builder,
        Assembly? migrationAssembly = null,
        Action<DbContextOptionsBuilder>? dbContextBuilder = null,
        Action<PostgresOptions>? configurator = null,
        params Assembly[] assembliesToScan
    )
        where TDbContext : DbContext, IDbFacadeResolver, IDomainEventContext
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        // Add an option to the dependency injection
        builder.Services.AddValidationOptions(configurator: configurator);

        builder.Services.AddScoped<IConnectionFactory>(sp =>
        {
            var postgresOptions = sp.GetRequiredService<IOptions<PostgresOptions>>().Value;
            postgresOptions.ConnectionString.NotBeNullOrWhiteSpace();
            return new NpgsqlConnectionFactory(postgresOptions.ConnectionString);
        });

        builder.Services.AddDbContext<TDbContext>(
            (sp, options) =>
            {
                var postgresOptions = sp.GetRequiredService<IOptions<PostgresOptions>>().Value;

                // https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-9.0/breaking-changes#pending-model-changes
                // https://github.com/dotnet/efcore/issues/35158
                options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));

                options
                    .UseNpgsql(
                        postgresOptions.ConnectionString,
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

        builder.Services.AddScoped<IDbFacadeResolver>(provider => provider.GetService<TDbContext>()!);
        builder.Services.AddScoped<IDomainEventContext>(provider => provider.GetService<TDbContext>()!);

        builder.AddPostgresRepositories(assembliesToScan);
        builder.AddPostgresUnitOfWork(assembliesToScan);

        builder
            .Services.AddHealthChecks()
            .AddNpgSql(
                sp =>
                {
                    var postgresOptions = sp.GetRequiredService<IOptions<PostgresOptions>>().Value;
                    return postgresOptions.ConnectionString;
                },
                name: "Postgres-Check",
                tags: ["live"]
            );

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
