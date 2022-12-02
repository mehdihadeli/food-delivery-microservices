using System.Reflection;
using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Events;
using BuildingBlocks.Abstractions.CQRS.Events.Internal;
using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Abstractions.Persistence.EfCore;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Core.Persistence.EfCore;
using BuildingBlocks.Core.Persistence.EfCore.Interceptors;
using Core.Persistence.Postgres;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace BuildingBlocks.Persistence.EfCore.Postgres;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPostgresDbContext<TDbContext>(
        this IServiceCollection services,
        Assembly? migrationAssembly = null,
        Action<DbContextOptionsBuilder>? builder = null)
        where TDbContext : DbContext, IDbFacadeResolver, IDomainEventContext
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.AddValidatedOptions<PostgresOptions>(nameof(PostgresOptions));

        services.AddScoped<IConnectionFactory>(sp =>
        {
            var postgresOptions = sp.GetService<PostgresOptions>();
            Guard.Against.NullOrEmpty(postgresOptions?.ConnectionString);
            return new NpgsqlConnectionFactory(postgresOptions.ConnectionString);
        });

        services.AddDbContext<TDbContext>((sp, options) =>
        {
            var postgresOptions = sp.GetRequiredService<PostgresOptions>();

            options.UseNpgsql(postgresOptions.ConnectionString, sqlOptions =>
                {
                    var name =
                        migrationAssembly?.GetName().Name ?? postgresOptions.MigrationAssembly ??
                        typeof(TDbContext).Assembly.GetName().Name;

                    sqlOptions.MigrationsAssembly(name);
                    sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                })

                // https://github.com/efcore/EFCore.NamingConventions
                .UseSnakeCaseNamingConvention();

            options.AddInterceptors(new AuditInterceptor(), new SoftDeleteInterceptor(), new ConcurrencyInterceptor());

            builder?.Invoke(options);
        });

        services.AddScoped<IDbFacadeResolver>(provider => provider.GetService<TDbContext>()!);
        services.AddScoped<IDomainEventContext>(provider => provider.GetService<TDbContext>()!);
        services.AddScoped<IDomainEventsAccessor, EfDomainEventAccessor>();

        return services;
    }

    public static IServiceCollection AddPostgresCustomRepository(
        this IServiceCollection services,
        Type customRepositoryType)
    {
        services.Scan(scan => scan
            .FromAssembliesOf(customRepositoryType)
            .AddClasses(classes =>
                classes.AssignableTo(customRepositoryType)).As(typeof(IRepository<,>)).WithScopedLifetime()
            .AddClasses(classes =>
                classes.AssignableTo(customRepositoryType)).As(typeof(IPageRepository<>)).WithScopedLifetime()
        );

        return services;
    }

    public static IServiceCollection AddPostgresRepository<TEntity, TKey, TRepository>(
        this IServiceCollection services,
        ServiceLifetime lifeTime = ServiceLifetime.Scoped)
        where TEntity : class, IAggregate<TKey>
        where TRepository : class, IRepository<TEntity, TKey>
    {
        return services.RegisterService<IRepository<TEntity, TKey>, TRepository>(lifeTime);
    }

    public static IServiceCollection AddUnitOfWork<TContext>(
        this IServiceCollection services,
        ServiceLifetime lifeTime = ServiceLifetime.Scoped,
        bool registerGeneric = false)
        where TContext : EfDbContextBase
    {
        if (registerGeneric)
        {
            services.RegisterService<IUnitOfWork, EfUnitOfWork<TContext>>(lifeTime);
        }

        return services.RegisterService<IEfUnitOfWork<TContext>, EfUnitOfWork<TContext>>(lifeTime);
    }

    private static IServiceCollection RegisterService<TService, TImplementation>(
        this IServiceCollection services,
        ServiceLifetime lifeTime = ServiceLifetime.Scoped)
        where TService : class
        where TImplementation : class, TService
    {
        ServiceDescriptor serviceDescriptor = lifeTime switch
        {
            ServiceLifetime.Singleton => ServiceDescriptor.Singleton<TService, TImplementation>(),
            ServiceLifetime.Scoped => ServiceDescriptor.Scoped<TService, TImplementation>(),
            ServiceLifetime.Transient => ServiceDescriptor.Transient<TService, TImplementation>(),
            _ => throw new ArgumentOutOfRangeException(nameof(lifeTime), lifeTime, null)
        };
        services.Add(serviceDescriptor);
        return services;
    }
}
