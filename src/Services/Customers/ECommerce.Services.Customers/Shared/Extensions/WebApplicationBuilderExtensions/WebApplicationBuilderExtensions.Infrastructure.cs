using System.Threading.RateLimiting;
using Ardalis.GuardClauses;
using BuildingBlocks.Caching;
using BuildingBlocks.Caching.Behaviours;
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

namespace ECommerce.Services.Customers.Shared.Extensions.WebApplicationBuilderExtensions;

public static partial class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        SnowFlakIdGenerator.Configure(2);

        builder.Services.AddCore(builder.Configuration);

        builder.Services.AddMonitoring(healthChecksBuilder =>
        {
            var postgresOptions = builder.Configuration.GetOptions<PostgresOptions>(nameof(PostgresOptions));
            var rabbitMqOptions = builder.Configuration.GetOptions<RabbitMqOptions>(nameof(RabbitMqOptions));
            var mongoOptions = builder.Configuration.GetOptions<MongoOptions>(nameof(MongoOptions));
            var identityApiClientOptions =
                builder.Configuration.GetOptions<IdentityApiClientOptions>(nameof(IdentityApiClientOptions));
            var catalogsApiClientOptions =
                builder.Configuration.GetOptions<CatalogsApiClientOptions>(nameof(CatalogsApiClientOptions));

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

        builder.Services.AddEmailService(builder.Configuration);

        builder.Services.AddCqrs(pipelines: new[]
        {
            typeof(RequestValidationBehavior<,>), typeof(StreamRequestValidationBehavior<,>),
            typeof(StreamLoggingBehavior<,>), typeof(StreamCachingBehavior<,>), typeof(LoggingBehavior<,>),
            typeof(CachingBehavior<,>), typeof(InvalidateCachingBehavior<,>), typeof(EfTxBehavior<,>)
        });

        builder.Services.AddPostgresMessagePersistence(builder.Configuration);

        // https://blog.maartenballiauw.be/post/2022/09/26/aspnet-core-rate-limiting-middleware.html
        builder.Services.AddRateLimiter(options =>
        {
            // rate limiter that limits all to 10 requests per minute, per authenticated username (or hostname if not authenticated)
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true, PermitLimit = 10, QueueLimit = 0, Window = TimeSpan.FromMinutes(1)
                    }));
        });

        builder.Services.AddCustomMassTransit(
            builder.Configuration,
            builder.Environment,
            (context, cfg) =>
            {
                cfg.AddUsersEndpoints(context);
                cfg.AddProductEndpoints(context);

                cfg.AddCustomerPublishers();
                cfg.AddRestockSubscriptionPublishers();
            },
            autoConfigEndpoints: false);

        builder.Services.AddCustomValidators(Assembly.GetExecutingAssembly());

        builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

        builder.AddCustomCaching();

        builder.AddCustomHttpClients();

        return builder;
    }
}
