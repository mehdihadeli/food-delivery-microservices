using BuildingBlocks.Caching.Behaviours;
using BuildingBlocks.Caching.Extensions;
using BuildingBlocks.Core.Extensions;
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
using BuildingBlocks.Web.RateLimit;
using BuildingBlocks.Web.Versioning;
using FoodDelivery.Services.Catalogs.Products;
using MessageHeaders = BuildingBlocks.Core.Messaging.MessageHeaders;

namespace FoodDelivery.Services.Catalogs.Shared.Extensions.WebApplicationBuilderExtensions;

public static partial class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        builder.Services.AddCore();

        builder.Services.AddCustomJwtAuthentication(builder.Configuration);
        builder.Services.AddCustomAuthorization(
            rolePolicies: new List<RolePolicy>
            {
                new(CatalogConstants.Role.Admin, new List<string> { CatalogConstants.Role.Admin }),
                new(CatalogConstants.Role.User, new List<string> { CatalogConstants.Role.User })
            }
        );

        builder.Services.AddEmailService(builder.Configuration);

        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CatalogsMetadata).Assembly);
            cfg.AddOpenBehavior(typeof(StreamLoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(RequestValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(StreamRequestValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(StreamCachingBehavior<,>));
            cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
            cfg.AddOpenBehavior(typeof(InvalidateCachingBehavior<,>));
            cfg.AddOpenBehavior(typeof(EfTxBehavior<,>));
        });

        // https://github.com/tonerdo/dotnet-env
        DotNetEnv.Env.TraversePath().Load();

        // https://www.thorsten-hans.com/hot-reload-net-configuration-in-kubernetes-with-configmaps/
        // https://bartwullems.blogspot.com/2021/03/kubernetesoverride-appsettingsjson-file.html
        // if we need to reload app by change settings, we can use volume map for watching our new setting from config-files folder in the the volume or change appsettings.json file in the volume map
        var configFolder = builder.Configuration.GetValue<string>("ConfigurationFolder") ?? "config-files/";
        builder.Configuration.AddKeyPerFile(configFolder, true, true);

        // https://www.michaco.net/blog/EnvironmentVariablesAndConfigurationInASPNETCoreApps#environment-variables-and-configuration
        // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0#non-prefixed-environment-variables
        builder.Configuration.AddEnvironmentVariables("food_delivery_catalogs_env_");

        builder.AddCustomVersioning();

        builder.AddCustomSwagger();

        builder.AddCustomCors();

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddPostgresMessagePersistence(builder.Configuration);

        builder.AddCompression();

        builder.AddAppProblemDetails();

        builder.AddCustomSerilog();

        builder.AddCustomOpenTelemetry();

        builder.Services.AddHeaderPropagation(options =>
        {
            options.HeaderNames.Add(MessageHeaders.CorrelationId);
            options.HeaderNames.Add(MessageHeaders.CausationId);
        });

        // https://blog.maartenballiauw.be/post/2022/09/26/aspnet-core-rate-limiting-middleware.html
        builder.AddCustomRateLimit();

        builder.AddCustomMassTransit(
            (busRegistrationContext, busFactoryConfigurator) =>
            {
                busFactoryConfigurator.AddProductPublishers();
            }
        );

        if (builder.Environment.IsTest() == false)
        {
            builder.AddCustomHealthCheck(healthChecksBuilder =>
            {
                var postgresOptions = builder.Configuration.BindOptions<PostgresOptions>(nameof(PostgresOptions));
                var rabbitMqOptions = builder.Configuration.BindOptions<RabbitMqOptions>(nameof(RabbitMqOptions));

                postgresOptions.NotBeNull();
                rabbitMqOptions.NotBeNull();

                healthChecksBuilder
                    .AddNpgSql(
                        postgresOptions.ConnectionString,
                        name: "CatalogsService-Postgres-Check",
                        tags: ["postgres", "infra", "database", "catalogs-service", "live", "ready",]
                    )
                    .AddRabbitMQ(
                        rabbitMqOptions.ConnectionString,
                        name: "CatalogsService-RabbitMQ-Check",
                        timeout: TimeSpan.FromSeconds(3),
                        tags: ["rabbitmq", "infra", "bus", "catalogs-service", "live", "ready",]
                    );
            });
        }

        builder.Services.AddCustomValidators(Assembly.GetExecutingAssembly());
        builder.Services.AddAutoMapper(x =>
        {
            x.AddProfile<ProductMappers>();
        });

        builder.AddCustomEasyCaching();

        return builder;
    }
}
