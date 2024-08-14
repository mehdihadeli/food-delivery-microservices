using System.Threading.RateLimiting;
using BuildingBlocks.Caching;
using BuildingBlocks.Caching.Behaviours;
using BuildingBlocks.Caching.Extensions;
using BuildingBlocks.Core;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Persistence.EfCore;
using BuildingBlocks.Core.Web.Extensions;
using BuildingBlocks.Core.Web.HeaderPropagation.Extensions;
using BuildingBlocks.Email;
using BuildingBlocks.HealthCheck;
using BuildingBlocks.Integration.MassTransit;
using BuildingBlocks.Logging;
using BuildingBlocks.Messaging.Persistence.Postgres.Extensions;
using BuildingBlocks.OpenTelemetry;
using BuildingBlocks.Persistence.EfCore.Postgres;
using BuildingBlocks.Security.Jwt;
using BuildingBlocks.Swagger;
using BuildingBlocks.Validation;
using BuildingBlocks.Validation.Extensions;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Versioning;
using FoodDelivery.Services.Orders.Customers;

namespace FoodDelivery.Services.Orders.Shared.Extensions.WebApplicationBuilderExtensions;

internal static partial class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        builder.Services.AddCore();

        builder.Services.AddCustomJwtAuthentication(builder.Configuration);
        builder.Services.AddCustomAuthorization(
            rolePolicies: new List<RolePolicy>
            {
                new(OrdersConstants.Role.Admin, new List<string> { OrdersConstants.Role.Admin }),
                new(OrdersConstants.Role.User, new List<string> { OrdersConstants.Role.User })
            }
        );

        // https://www.michaco.net/blog/EnvironmentVariablesAndConfigurationInASPNETCoreApps#environment-variables-and-configuration
        // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0#non-prefixed-environment-variables
        builder.Configuration.AddEnvironmentVariables("food_delivery_orders_env_");

        // https://github.com/tonerdo/dotnet-env
        DotNetEnv.Env.TraversePath().Load();

        builder.AddCompression();

        builder.AddAppProblemDetails();

        builder.AddCustomSerilog();

        builder.AddCustomVersioning();

        builder.AddCustomSwagger(cfg =>
        {
            cfg.Name = "Orders Apis";
            cfg.Title = "Orders Apis";
        });

        builder.AddCustomCors();

        builder.Services.AddHttpContextAccessor();

        builder.AddCustomOpenTelemetry();

        builder.Services.AddHeaderPropagation(options =>
        {
            options.HeaderNames.Add(MessageHeaders.CorrelationId);
            options.HeaderNames.Add(MessageHeaders.CausationId);
        });

        if (builder.Environment.IsTest() == false)
        {
            builder.AddCustomHealthCheck(healthChecksBuilder =>
            {
                var postgresOptions = builder.Configuration.BindOptions<PostgresOptions>();
                var rabbitMqOptions = builder.Configuration.BindOptions<RabbitMqOptions>();

                postgresOptions.NotBeNull();
                rabbitMqOptions.NotBeNull();

                healthChecksBuilder
                    .AddNpgSql(
                        postgresOptions.ConnectionString,
                        name: "OrdersService-Postgres-Check",
                        tags: ["postgres", "database", "infra", "orders-service", "live", "ready",]
                    )
                    .AddRabbitMQ(
                        rabbitMqOptions.ConnectionString,
                        name: "OrdersService-RabbitMQ-Check",
                        timeout: TimeSpan.FromSeconds(3),
                        tags: ["rabbitmq", "bus", "infra", "orders-service", "live", "ready",]
                    );
            });
        }

        builder.Services.AddEmailService(builder.Configuration);

        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenStreamBehavior(typeof(StreamLoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(RequestValidationBehavior<,>));
            cfg.AddOpenStreamBehavior(typeof(StreamRequestValidationBehavior<,>));
            cfg.AddOpenStreamBehavior(typeof(StreamCachingBehavior<,>));
            cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
            cfg.AddOpenBehavior(typeof(InvalidateCachingBehavior<,>));
            cfg.AddOpenBehavior(typeof(EfTxBehavior<,>));
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
                        AutoReplenishment = true,
                        PermitLimit = 10,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1)
                    }
                )
            );
        });

        builder.AddCustomMassTransit(
            (context, cfg) =>
            {
                cfg.AddCustomerEndpoints(context);
            },
            autoConfigEndpoints: false
        );

        builder.Services.AddCustomValidators(Assembly.GetExecutingAssembly());

        builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

        builder.AddCustomEasyCaching();

        return builder;
    }
}
