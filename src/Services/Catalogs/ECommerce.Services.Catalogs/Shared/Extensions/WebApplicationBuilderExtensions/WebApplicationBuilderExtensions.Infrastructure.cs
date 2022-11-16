using System.Threading.RateLimiting;
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
using BuildingBlocks.OpenTelemetry;
using BuildingBlocks.Persistence.EfCore.Postgres;
using BuildingBlocks.Validation;
using ECommerce.Services.Catalogs.Products;

namespace ECommerce.Services.Catalogs.Shared.Extensions.WebApplicationBuilderExtensions;

public static partial class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        SnowFlakIdGenerator.Configure(1);

        builder.Services.AddCore(builder.Configuration);

        builder.Services.AddEmailService(builder.Configuration);
        builder.Services.AddCqrs(pipelines: new[]
        {
            typeof(RequestValidationBehavior<,>), typeof(StreamRequestValidationBehavior<,>),
            typeof(StreamLoggingBehavior<,>), typeof(StreamCachingBehavior<,>), typeof(LoggingBehavior<,>),
            typeof(CachingBehavior<,>), typeof(InvalidateCachingBehavior<,>), typeof(EfTxBehavior<,>)
        });

        builder.Services.AddPostgresMessagePersistence(builder.Configuration);

        builder.AddOTelTracing();
        builder.AddOTelMetrics();

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
            (busRegistrationContext, busFactoryConfigurator) =>
            {
                busFactoryConfigurator.AddProductPublishers();
            });

        builder.Services.AddMonitoring(healthChecksBuilder =>
        {
            var postgresOptions = builder.Configuration.GetOptions<PostgresOptions>(nameof(PostgresOptions));
            var rabbitMqOptions = builder.Configuration.GetOptions<RabbitMqOptions>(nameof(RabbitMqOptions));

            Guard.Against.Null(postgresOptions, nameof(postgresOptions));
            Guard.Against.Null(rabbitMqOptions, nameof(rabbitMqOptions));

            healthChecksBuilder
                .AddNpgSql(
                    postgresOptions.ConnectionString,
                    name: "CatalogsService-Postgres-Check",
                    tags: new[] {"postgres", "infra", "database", "catalogs-service"})
                .AddRabbitMQ(
                    rabbitMqOptions.ConnectionString,
                    name: "CatalogsService-RabbitMQ-Check",
                    timeout: TimeSpan.FromSeconds(3),
                    tags: new[] {"rabbitmq", "infra", "bus", "catalogs-service"});
        });

        builder.Services.AddCustomValidators(Assembly.GetExecutingAssembly());
        builder.Services.AddAutoMapper(x =>
        {
            x.AddProfile<ProductMappers>();
        });

        builder.Services.AddCustomInMemoryCache(builder.Configuration)
            .AddCachingRequestPolicies(Assembly.GetExecutingAssembly());

        return builder;
    }
}
