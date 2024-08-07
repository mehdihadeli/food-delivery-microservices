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
using BuildingBlocks.Persistence.Mongo;
using BuildingBlocks.Security.Extensions;
using BuildingBlocks.Security.Jwt;
using BuildingBlocks.Swagger;
using BuildingBlocks.Validation;
using BuildingBlocks.Validation.Extensions;
using BuildingBlocks.Web.Extensions;
using FoodDelivery.Services.Customers.Customers.Extensions;
using FoodDelivery.Services.Customers.Products;
using FoodDelivery.Services.Customers.RestockSubscriptions;
using FoodDelivery.Services.Customers.Shared.Clients.Catalogs;
using FoodDelivery.Services.Customers.Shared.Clients.Identity;
using FoodDelivery.Services.Customers.Users;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FoodDelivery.Services.Customers.Shared.Extensions.WebApplicationBuilderExtensions;

public static partial class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        builder.Services.AddCore(typeof(CustomersMetadata).Assembly);

        builder.Services.AddCustomJwtAuthentication(builder.Configuration);
        builder.Services.AddCustomAuthorization(
            rolePolicies: new List<RolePolicy>
            {
                new(CustomersConstants.Role.Admin, new List<string> { CustomersConstants.Role.Admin }),
                new(CustomersConstants.Role.User, new List<string> { CustomersConstants.Role.User })
            }
        );

        // https://www.michaco.net/blog/EnvironmentVariablesAndConfigurationInASPNETCoreApps#environment-variables-and-configuration
        // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0#non-prefixed-environment-variables
        builder.Configuration.AddEnvironmentVariables("food_delivery_customers_env_");

        // https://github.com/tonerdo/dotnet-env
        DotNetEnv.Env.TraversePath().Load();

        if (builder.Environment.IsTest() == false)
        {
            builder.AddCustomHealthCheck(healthChecksBuilder =>
            {
                var postgresOptions = builder.Configuration.BindOptions<PostgresOptions>();
                var rabbitMqOptions = builder.Configuration.BindOptions<RabbitMqOptions>();
                var mongoOptions = builder.Configuration.BindOptions<MongoOptions>();
                var identityApiClientOptions = builder.Configuration.BindOptions<IdentityApiClientOptions>(
                    "IdentityApiClientOptions"
                );
                var catalogsApiClientOptions = builder.Configuration.BindOptions<CatalogsApiClientOptions>();

                healthChecksBuilder
                    .AddNpgSql(
                        postgresOptions.ConnectionString,
                        name: "CustomersService-Postgres-Check",
                        tags: new[] { "postgres", "database", "infra", "customers-service", "live", "ready" }
                    )
                    .AddRabbitMQ(
                        rabbitMqOptions.ConnectionString,
                        name: "CustomersService-RabbitMQ-Check",
                        timeout: TimeSpan.FromSeconds(3),
                        tags: new[] { "rabbitmq", "bus", "infra", "customers-service", "live", "ready" }
                    )
                    .AddMongoDb(
                        mongoOptions.ConnectionString,
                        mongoDatabaseName: mongoOptions.DatabaseName,
                        "CustomersService-Mongo-Check",
                        tags: new[] { "mongodb", "database", "infra", "customers-service", "live", "ready" }
                    )
                    .AddUrlGroup(
                        new List<Uri> { new($"{catalogsApiClientOptions.BaseApiAddress}/healthz") },
                        name: "Catalogs-Downstream-API-Check",
                        failureStatus: HealthStatus.Unhealthy,
                        timeout: TimeSpan.FromSeconds(3),
                        tags: new[] { "uris", "downstream-services", "customers-service", "live", "ready" }
                    )
                    .AddUrlGroup(
                        new List<Uri> { new($"{identityApiClientOptions.BaseApiAddress}/healthz") },
                        name: "Identity-Downstream-API-Check",
                        failureStatus: HealthStatus.Unhealthy,
                        timeout: TimeSpan.FromSeconds(3),
                        tags: new[] { "uris", "downstream-services", "customers-service", "live", "ready" }
                    );
            });
        }

        builder.Services.AddEmailService(builder.Configuration);

        builder.AddCompression();
        builder.AddAppProblemDetails();

        builder.AddCustomOpenTelemetry();

        builder.AddCustomSerilog();

        builder.AddCustomCors();

        builder.AddCustomVersioning();
        builder.AddCustomSwagger();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddCqrs(
            pipelines: new[]
            {
                typeof(LoggingBehavior<,>),
                typeof(StreamLoggingBehavior<,>),
                typeof(RequestValidationBehavior<,>),
                typeof(StreamRequestValidationBehavior<,>),
                typeof(StreamCachingBehavior<,>),
                typeof(CachingBehavior<,>),
                typeof(InvalidateCachingBehavior<,>),
                typeof(EfTxBehavior<,>)
            }
        );

        builder.Services.AddPostgresMessagePersistence(builder.Configuration);

        // https://blog.maartenballiauw.be/post/2022/09/26/aspnet-core-rate-limiting-middleware.html
        builder.AddCustomRateLimit();

        builder.AddCustomMassTransit(
            (context, cfg) =>
            {
                cfg.AddUsersEndpoints(context);
                cfg.AddProductEndpoints(context);

                cfg.AddCustomerPublishers();
                cfg.AddRestockSubscriptionPublishers();
            },
            autoConfigEndpoints: false
        );

        builder.Services.AddCustomValidators(Assembly.GetExecutingAssembly());

        builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

        builder.AddCustomEasyCaching();

        builder.AddCustomHttpClients();

        return builder;
    }
}
