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
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Persistence.EfCore.Postgres;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddPostgresDbContext<TDbContext>(
        this IServiceCollection services,
        Assembly? migrationAssembly = null,
        Action<DbContextOptionsBuilder>? builder = null,
        Action<PostgresOptions>? configurator = null,
        params Assembly[] assembliesToScan
    )
        where TDbContext : DbContext, IDbFacadeResolver, IDomainEventContext
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        // Add option to the dependency injection
        services.AddValidationOptions(configurator: configurator);

        services.AddScoped<IConnectionFactory>(sp =>
        {
            var postgresOptions = sp.GetRequiredService<IOptions<PostgresOptions>>().Value;
            postgresOptions.ConnectionString.NotBeNullOrWhiteSpace();
            return new NpgsqlConnectionFactory(postgresOptions.ConnectionString);
        });

        services.AddDbContext<TDbContext>(
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

                builder?.Invoke(options);
            }
        );

        services.AddScoped<IDbFacadeResolver>(provider => provider.GetService<TDbContext>()!);
        services.AddScoped<IDomainEventContext>(provider => provider.GetService<TDbContext>()!);

        services.AddPostgresRepositories(assembliesToScan);
        services.AddPostgresUnitOfWork(assembliesToScan);

        return services;
    }

    private static IServiceCollection AddPostgresRepositories(
        this IServiceCollection services,
        params Assembly[] assembliesToScan
    )
    {
        var scanAssemblies = assembliesToScan.Length != 0 ? assembliesToScan : [Assembly.GetCallingAssembly()];
        services.Scan(scan =>
            scan.FromAssemblies(scanAssemblies)
                .AddClasses(classes => classes.AssignableTo(typeof(IRepository<,>)), false)
                .AsImplementedInterfaces()
                .AsSelf()
                .WithTransientLifetime()
        );

        return services;
    }

    private static IServiceCollection AddPostgresUnitOfWork(
        this IServiceCollection services,
        params Assembly[] assembliesToScan
    )
    {
        var scanAssemblies = assembliesToScan.Length != 0 ? assembliesToScan : [Assembly.GetCallingAssembly()];
        services.Scan(scan =>
            scan.FromAssemblies(scanAssemblies)
                .AddClasses(classes => classes.AssignableTo(typeof(IEfUnitOfWork<>)), false)
                .AsImplementedInterfaces()
                .AsSelf()
                .WithTransientLifetime()
        );

        return services;
    }
}
