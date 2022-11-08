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
using BuildingBlocks.Validation;
using ECommerce.Services.Orders.Customers;

namespace ECommerce.Services.Orders.Shared.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment)
    {
        SnowFlakIdGenerator.Configure(3);

        services.AddCore(configuration);

        services.AddMonitoring(healthChecksBuilder =>
        {
            var postgresOptions = configuration.GetOptions<PostgresOptions>(nameof(PostgresOptions));
            var rabbitMqOptions = configuration.GetOptions<RabbitMqOptions>(nameof(RabbitMqOptions));

            Guard.Against.Null(postgresOptions, nameof(postgresOptions));
            Guard.Against.Null(rabbitMqOptions, nameof(rabbitMqOptions));

            healthChecksBuilder
                .AddNpgSql(
                    postgresOptions.ConnectionString,
                    name: "OrdersService-Postgres-Check",
                    tags: new[] {"postgres", "database", "infra", "orders-service"})
                .AddRabbitMQ(
                    rabbitMqOptions.ConnectionString,
                    name: "OrdersService-RabbitMQ-Check",
                    timeout: TimeSpan.FromSeconds(3),
                    tags: new[] {"rabbitmq", "bus", "infra", "orders-service"});
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
                cfg.AddCustomerEndpoints(context);
            },
            autoConfigEndpoints: false);

        services.AddCustomValidators(Assembly.GetExecutingAssembly());

        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddCustomInMemoryCache(configuration)
            .AddCachingRequestPolicies(Assembly.GetExecutingAssembly());

        return services;
    }
}
