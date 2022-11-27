using System.Threading.RateLimiting;
using Ardalis.GuardClauses;
using BuildingBlocks.Caching;
using BuildingBlocks.Caching.Behaviours;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.IdsGenerator;
using BuildingBlocks.Core.Persistence.EfCore;
using BuildingBlocks.Core.Registrations;
using BuildingBlocks.Email;
using BuildingBlocks.HealthCheck;
using BuildingBlocks.Integration.MassTransit;
using BuildingBlocks.Logging;
using BuildingBlocks.Messaging.Persistence.Postgres.Extensions;
using BuildingBlocks.OpenTelemetry;
using BuildingBlocks.Persistence.EfCore.Postgres;
using BuildingBlocks.Security.Extensions;
using BuildingBlocks.Security.Jwt;
using BuildingBlocks.Swagger;
using BuildingBlocks.Validation;
using BuildingBlocks.Web.Extensions;
using ECommerce.Services.Orders.Customers;
using Serilog.Events;

namespace ECommerce.Services.Orders.Shared.Extensions.WebApplicationBuilderExtensions;

internal static partial class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        SnowFlakIdGenerator.Configure(3);

        builder.Services.AddCore(builder.Configuration);

        builder.Services.AddCustomJwtAuthentication(builder.Configuration);
        builder.Services.AddCustomAuthorization(
            rolePolicies: new List<RolePolicy>
            {
                new(OrdersConstants.Role.Admin, new List<string> {OrdersConstants.Role.Admin}),
                new(OrdersConstants.Role.User, new List<string> {OrdersConstants.Role.User})
            });

        // https://www.michaco.net/blog/EnvironmentVariablesAndConfigurationInASPNETCoreApps#environment-variables-and-configuration
        // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0#non-prefixed-environment-variables
        builder.Configuration.AddEnvironmentVariables("ecommerce_orders_env_");

        // https://github.com/tonerdo/dotnet-env
        DotNetEnv.Env.TraversePath().Load();

        builder.AddCompression();

        builder.AddCustomProblemDetails();

        builder.AddCustomSerilog(
            optionsBuilder =>
            {
                optionsBuilder.SetLevel(LogEventLevel.Information);
            });

        builder.AddCustomVersioning();

        builder.AddCustomSwagger(typeof(OrdersRoot).Assembly);

        builder.Services.AddHttpContextAccessor();

        builder.AddCustomOpenTelemetry();

        if (builder.Environment.IsTest() == false)
        {
            builder.Services.AddCustomHealthCheck(healthChecksBuilder =>
            {
                var postgresOptions = builder.Configuration.GetOptions<PostgresOptions>(nameof(PostgresOptions));
                var rabbitMqOptions = builder.Configuration.GetOptions<RabbitMqOptions>(nameof(RabbitMqOptions));

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
        }

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
                cfg.AddCustomerEndpoints(context);
            },
            autoConfigEndpoints: false);

        builder.Services.AddCustomValidators(Assembly.GetExecutingAssembly());

        builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

        builder.AddCustomCaching();

        return builder;
    }
}
