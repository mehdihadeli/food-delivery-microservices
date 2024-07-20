using System.Reflection;
using BuildingBlocks.Abstractions.Domain.Events.Internal;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Abstractions.Persistence.EfCore;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Core.Persistence.EfCore;
using BuildingBlocks.Core.Persistence.EfCore.Interceptors;
using Core.Persistence.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlocks.Persistence.EfCore.Postgres;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPostgresDbContext<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly? migrationAssembly = null,
        Action<DbContextOptionsBuilder>? builder = null,
        Action<PostgresOptions>? configurator = null,
        params Assembly[] assembliesToScan
    )
        where TDbContext : DbContext, IDbFacadeResolver, IDomainEventContext
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var options = configuration.BindOptions<PostgresOptions>();
        configurator?.Invoke(options);

        // add option to the dependency injection
        services.AddValidationOptions<PostgresOptions>(opt => configurator?.Invoke(opt));

        services.TryAddScoped<IConnectionFactory>(
            sp => new NpgsqlConnectionFactory(options.ConnectionString.NotBeEmptyOrNull())
        );

        services.AddDbContext<TDbContext>(
            (sp, dbContextOptionsBuilder) =>
            {
                dbContextOptionsBuilder
                    .UseNpgsql(
                        options.ConnectionString,
                        sqlOptions =>
                        {
                            var name =
                                migrationAssembly?.GetName().Name
                                ?? options.MigrationAssembly
                                ?? typeof(TDbContext).Assembly.GetName().Name;

                            sqlOptions.MigrationsAssembly(name);
                            sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                        }
                    )
                    // https://github.com/efcore/EFCore.NamingConventions
                    .UseSnakeCaseNamingConvention();

                // ref: https://andrewlock.net/series/using-strongly-typed-entity-ids-to-avoid-primitive-obsession/
                dbContextOptionsBuilder.ReplaceService<
                    IValueConverterSelector,
                    StronglyTypedIdValueConverterSelector<long>
                >();

                dbContextOptionsBuilder.AddInterceptors(
                    new AuditInterceptor(),
                    new SoftDeleteInterceptor(),
                    new ConcurrencyInterceptor()
                );

                builder?.Invoke(dbContextOptionsBuilder);
            }
        );

        services.TryAddScoped<IDbFacadeResolver>(provider => provider.GetService<TDbContext>()!);
        services.TryAddScoped<IDomainEventContext>(provider => provider.GetService<TDbContext>()!);

        services.AddPostgresRepositories(assembliesToScan);
        services.AddPostgresUnitOfWork(assembliesToScan);

        return services;
    }

    private static IServiceCollection AddPostgresRepositories(
        this IServiceCollection services,
        params Assembly[] assembliesToScan
    )
    {
        var scanAssemblies = assembliesToScan.Any() ? assembliesToScan : new[] { Assembly.GetCallingAssembly() };
        services.Scan(
            scan =>
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
        var scanAssemblies = assembliesToScan.Any() ? assembliesToScan : new[] { Assembly.GetCallingAssembly() };
        services.Scan(
            scan =>
                scan.FromAssemblies(scanAssemblies)
                    .AddClasses(classes => classes.AssignableTo(typeof(IEfUnitOfWork<>)), false)
                    .AsImplementedInterfaces()
                    .AsSelf()
                    .WithTransientLifetime()
        );

        return services;
    }
}
