using System.Threading.RateLimiting;
using Ardalis.GuardClauses;
using BuildingBlocks.Caching.InMemory;
using BuildingBlocks.Core.Caching;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Persistence.EfCore;
using BuildingBlocks.Core.Registrations;
using BuildingBlocks.Email;
using BuildingBlocks.Integration.MassTransit;
using BuildingBlocks.Logging;
using BuildingBlocks.Messaging.Persistence.Postgres.Extensions;
using BuildingBlocks.Monitoring;
using BuildingBlocks.Persistence.EfCore.Postgres;
using BuildingBlocks.Validation;
using ECommerce.Services.Identity.Users;

namespace ECommerce.Services.Identity.Shared.Extensions.WebApplicationBuilderExtensions;

public static partial class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        builder.Services.AddCore(builder.Configuration);

        builder.Services.AddMonitoring(healthChecksBuilder =>
        {
            var postgresOptions = builder.Configuration.GetOptions<PostgresOptions>(nameof(PostgresOptions));
            var rabbitMqOptions = builder.Configuration.GetOptions<RabbitMqOptions>(nameof(RabbitMqOptions));

            Guard.Against.Null(postgresOptions, nameof(postgresOptions));
            Guard.Against.Null(rabbitMqOptions, nameof(rabbitMqOptions));

            healthChecksBuilder
                .AddNpgSql(
                    postgresOptions.ConnectionString,
                    name: "IdentityService-Postgres-Check",
                    tags: new[] {"postgres", "database", "infra", "identity-service"})
                .AddRabbitMQ(
                    rabbitMqOptions.ConnectionString,
                    name: "IdentityService-RabbitMQ-Check",
                    timeout: TimeSpan.FromSeconds(3),
                    tags: new[] {"rabbitmq", "bus", "infra", "identity-service"});
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
                cfg.AddUserPublishers();
            },
            autoConfigEndpoints: false);

        builder.Services.AddCustomInMemoryCache(builder.Configuration)
            .AddCachingRequestPolicies(Assembly.GetExecutingAssembly());


        builder.Services.AddCustomValidators(Assembly.GetExecutingAssembly());

        builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

        return builder;
    }
}
