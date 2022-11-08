using Ardalis.GuardClauses;
using BuildingBlocks.Caching.InMemory;
using BuildingBlocks.Core.Caching;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.IdsGenerator;
using BuildingBlocks.Core.Persistence.EfCore;
using BuildingBlocks.Core.Registrations;
using BuildingBlocks.Email;
using BuildingBlocks.Integration.MassTransit;
using BuildingBlocks.Logging;
using BuildingBlocks.Messaging.Persistence.Postgres.Extensions;
using BuildingBlocks.Monitoring;
using BuildingBlocks.Persistence.EfCore.Postgres;
using BuildingBlocks.Persistence.Mongo;
using BuildingBlocks.Validation;
using ECommerce.Services.Customers.Customers;
using ECommerce.Services.Customers.Products;
using ECommerce.Services.Customers.RestockSubscriptions;
using ECommerce.Services.Customers.Shared.Clients.Catalogs;
using ECommerce.Services.Customers.Shared.Clients.Identity;
using ECommerce.Services.Customers.Users;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ECommerce.Services.Customers.Shared.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment)
    {
        SnowFlakIdGenerator.Configure(2);

        services.AddCore(configuration);

        services.AddMonitoring(healthChecksBuilder =>
        {
            var postgresOptions = configuration.GetOptions<PostgresOptions>(nameof(PostgresOptions));
            var rabbitMqOptions = configuration.GetOptions<RabbitMqOptions>(nameof(RabbitMqOptions));
            var mongoOptions = configuration.GetOptions<MongoOptions>(nameof(MongoOptions));
            var identityApiClientOptions =
                configuration.GetOptions<IdentityApiClientOptions>(nameof(IdentityApiClientOptions));
            var catalogsApiClientOptions =
                configuration.GetOptions<CatalogsApiClientOptions>(nameof(CatalogsApiClientOptions));

            Guard.Against.Null(postgresOptions, nameof(postgresOptions));
            Guard.Against.Null(rabbitMqOptions, nameof(rabbitMqOptions));
            Guard.Against.Null(mongoOptions, nameof(mongoOptions));
            Guard.Against.Null(identityApiClientOptions, nameof(identityApiClientOptions));
            Guard.Against.Null(catalogsApiClientOptions, nameof(catalogsApiClientOptions));

            healthChecksBuilder
                .AddNpgSql(
                    postgresOptions.ConnectionString,
                    name: "CustomersService-Postgres-Check",
                    tags: new[] {"postgres", "database", "infra", "customers-service"})
                .AddRabbitMQ(
                    rabbitMqOptions.ConnectionString,
                    name: "CustomersService-RabbitMQ-Check",
                    timeout: TimeSpan.FromSeconds(3),
                    tags: new[] {"rabbitmq", "bus", "infra", "customers-service"})
                .AddMongoDb(
                    mongoOptions.ConnectionString,
                    mongoDatabaseName: mongoOptions.DatabaseName,
                    "CustomersService-Mongo-Check",
                    tags: new[] {"mongodb", "database", "infra", "customers-service"})
                .AddUrlGroup(
                    new List<Uri> {new($"{catalogsApiClientOptions.BaseApiAddress}/healthz")},
                    name: "Catalogs-Downstream-API-Check",
                    failureStatus: HealthStatus.Unhealthy,
                    timeout: TimeSpan.FromSeconds(3),
                    tags: new[] {"uris", "downstream-services", "customers-service"})
                .AddUrlGroup(
                    new List<Uri> {new($"{identityApiClientOptions.BaseApiAddress}/healthz")},
                    name: "Identity-Downstream-API-Check",
                    failureStatus: HealthStatus.Unhealthy,
                    timeout: TimeSpan.FromSeconds(3),
                    tags: new[] {"uris", "downstream-services", "customers-service"});
        });

        services.AddEmailService(configuration);

        services.AddCqrs(pipelines: new[]
        {
            typeof(RequestValidationBehavior<,>), typeof(StreamRequestValidationBehavior<,>),
            typeof(StreamLoggingBehavior<,>), typeof(StreamCachingBehavior<,>), typeof(LoggingBehavior<,>),
            typeof(CachingBehavior<,>), typeof(InvalidateCachingBehavior<,>), typeof(EfTxBehavior<,>)
        });

        services.AddPostgresMessagePersistence(configuration);

        services.AddCustomMassTransit(
            configuration,
            webHostEnvironment,
            (context, cfg) =>
            {
                cfg.AddUsersEndpoints(context);
                cfg.AddProductEndpoints(context);

                cfg.AddCustomerPublishers();
                cfg.AddRestockSubscriptionPublishers();
            },
            autoConfigEndpoints: false);

        services.AddCustomValidators(Assembly.GetExecutingAssembly());

        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddCustomInMemoryCache(configuration)
            .AddCachingRequestPolicies(Assembly.GetExecutingAssembly());

        services.AddCustomHttpClients(configuration);

        return services;
    }
}
