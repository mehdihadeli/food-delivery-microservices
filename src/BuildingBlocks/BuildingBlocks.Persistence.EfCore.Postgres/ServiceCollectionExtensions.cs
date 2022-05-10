using System.Reflection;
using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Event;
using BuildingBlocks.Abstractions.CQRS.Event.Internal;
using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Abstractions.Persistence.EfCore;
using BuildingBlocks.Core.Persistence.EfCore;
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
        IConfiguration configuration,
        Assembly? migrationAssembly = null,
        Action<PostgresOptions>? configurator = null,
        Action<DbContextOptionsBuilder>? builder = null)
        where TDbContext : DbContext, IDbFacadeResolver, IDomainEventContext
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var config = configuration.GetSection(nameof(PostgresOptions)).Get<PostgresOptions>();

        services.Configure<PostgresOptions>(configuration.GetSection(nameof(PostgresOptions)));
        if (configurator is { })
            services.Configure(nameof(PostgresOptions), configurator);

        Guard.Against.NullOrEmpty(config.ConnectionString, nameof(config.ConnectionString));

        services.AddDbContext<TDbContext>(options =>
        {
            options.UseNpgsql(config.ConnectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly((migrationAssembly ?? typeof(TDbContext).Assembly).GetName().Name);
                sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            }).UseSnakeCaseNamingConvention();

            builder?.Invoke(options);
        });

        services.AddScoped<IConnectionFactory, NpgsqlConnectionFactory>();

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


    public static void MigrateDataFromScript(this MigrationBuilder migrationBuilder)
    {
        var assembly = Assembly.GetCallingAssembly();
        var files = assembly.GetManifestResourceNames();
        var filePrefix = $"{assembly.GetName().Name}.Data.Scripts.";

        foreach (var file in files
                     .Where(f => f.StartsWith(filePrefix) && f.EndsWith(".sql"))
                     .Select(f => new {PhysicalFile = f, LogicalFile = f.Replace(filePrefix, string.Empty)})
                     .OrderBy(f => f.LogicalFile))
        {
            using var stream = assembly.GetManifestResourceStream(file.PhysicalFile);
            using var reader = new StreamReader(stream!);
            var command = reader.ReadToEnd();

            if (string.IsNullOrWhiteSpace(command))
                continue;

            migrationBuilder.Sql(command);
        }
    }

    public static async Task DoDbMigrationAsync(
        this IApplicationBuilder app,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var scope = app.ApplicationServices.CreateAsyncScope();
        var dbFacadeResolver = scope.ServiceProvider.GetService<IDbFacadeResolver>();

        var policy = CreatePolicy(3, logger, "postgres");
        await policy.ExecuteAsync(async () =>
        {
            if (!await dbFacadeResolver?.Database.CanConnectAsync(cancellationToken)!)
            {
                Console.WriteLine($"Connection String: {dbFacadeResolver?.Database.GetConnectionString()}");
                throw new System.Exception("Couldn't connect database.");
            }

            var migrations =
                await dbFacadeResolver?.Database.GetPendingMigrationsAsync(cancellationToken: cancellationToken)!;
            if (migrations.Any())
            {
                await dbFacadeResolver?.Database.MigrateAsync(cancellationToken: cancellationToken)!;
                logger?.LogInformation("Migration database schema. Done!!!");
            }
        });

        static AsyncRetryPolicy CreatePolicy(int retries, ILogger logger, string prefix)
        {
            return Policy.Handle<System.Exception>().WaitAndRetryAsync(
                retries,
                retry => TimeSpan.FromSeconds(15),
                (exception, timeSpan, retry, ctx) =>
                {
                    logger.LogWarning(
                        exception,
                        "[{Prefix}] Exception {ExceptionType} with message {Message} detected on attempt {Retry} of {Retries}",
                        prefix,
                        exception.GetType().Name,
                        exception.Message,
                        retry,
                        retries);
                }
            );
        }
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
