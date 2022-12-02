using BuildingBlocks.Caching;
using BuildingBlocks.Caching.Behaviours;
using BuildingBlocks.Core.Extensions;
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
using ECommerce.Services.Identity.Users;
using Serilog.Events;

namespace ECommerce.Services.Identity.Shared.Extensions.WebApplicationBuilderExtensions;

public static partial class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        builder.Services.AddCore(builder.Configuration);

        builder.Services.AddCustomJwtAuthentication(builder.Configuration);
        builder.Services.AddCustomAuthorization(
            rolePolicies: new List<RolePolicy>
            {
                new(IdentityConstants.Role.Admin, new List<string> {IdentityConstants.Role.Admin}),
                new(IdentityConstants.Role.User, new List<string> {IdentityConstants.Role.User})
            });

        // https://www.michaco.net/blog/EnvironmentVariablesAndConfigurationInASPNETCoreApps#environment-variables-and-configuration
        // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0#non-prefixed-environment-variables
        builder.Configuration.AddEnvironmentVariables("ecommerce_identity_env_");

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

        builder.AddCustomSwagger(typeof(IdentityRoot).Assembly);

        builder.AddCustomOpenTelemetry();

        builder.Services.AddHttpContextAccessor();

        if (builder.Environment.IsTest() == false)
        {
            builder.Services.AddCustomHealthCheck(healthChecksBuilder =>
            {
                var postgresOptions = builder.Configuration.GetOptions<PostgresOptions>();
                var rabbitMqOptions = builder.Configuration.GetOptions<RabbitMqOptions>();

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
        builder.AddCustomRateLimit();

        builder.Services.AddCustomMassTransit(
            builder.Environment,
            (context, cfg) =>
            {
                cfg.AddUserPublishers();
            },
            autoConfigEndpoints: false);

        builder.AddCustomCaching();

        builder.Services.AddCustomValidators(Assembly.GetExecutingAssembly());

        builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

        return builder;
    }
}
