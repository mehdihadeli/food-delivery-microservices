using BuildingBlocks.Caching.Behaviours;
using BuildingBlocks.Caching.Extensions;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Persistence.EfCore;
using BuildingBlocks.Core.Web.HeaderPropagation.Extensions;
using BuildingBlocks.Email;
using BuildingBlocks.Integration.MassTransit;
using BuildingBlocks.Logging;
using BuildingBlocks.Messaging.Persistence.Postgres.Extensions;
using BuildingBlocks.OpenTelemetry;
using BuildingBlocks.Security.Jwt;
using BuildingBlocks.Swagger;
using BuildingBlocks.Validation;
using BuildingBlocks.Validation.Extensions;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.RateLimit;
using BuildingBlocks.Web.Versioning;
using FoodDelivery.Services.Customers.Customers;
using FoodDelivery.Services.Customers.Products;
using FoodDelivery.Services.Customers.RestockSubscriptions;
using FoodDelivery.Services.Customers.Shared.Clients;
using FoodDelivery.Services.Customers.Users;

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

        // if (builder.Environment.IsTest() == false)
        // {
        //     builder.AddCustomHealthCheck(healthChecksBuilder =>
        //     {
        //         var postgresOptions = builder.Configuration.BindOptions<PostgresOptions>();
        //         var rabbitMqOptions = builder.Configuration.BindOptions<RabbitMqOptions>();
        //         var mongoOptions = builder.Configuration.BindOptions<MongoOptions>();
        //         var identityApiClientOptions = builder.Configuration.BindOptions<IdentityApiClientOptions>(
        //             "IdentityApiClientOptions"
        //         );
        //         var catalogsApiClientOptions = builder.Configuration.BindOptions<CatalogsApiClientOptions>();
        //
        //         healthChecksBuilder
        //             .AddNpgSql(
        //                 postgresOptions.ConnectionString,
        //                 name: "CustomersService-Postgres-Check",
        //                 tags: ["postgres", "database", "infra", "customers-service", "live", "ready",]
        //             )
        //             .AddRabbitMQ(
        //                 rabbitMqOptions.ConnectionString,
        //                 name: "CustomersService-RabbitMQ-Check",
        //                 timeout: TimeSpan.FromSeconds(3),
        //                 tags: ["rabbitmq", "bus", "infra", "customers-service", "live", "ready",]
        //             )
        //             .AddMongoDb(
        //                 mongoOptions.ConnectionString,
        //                 mongoDatabaseName: mongoOptions.DatabaseName,
        //                 "CustomersService-Mongo-Check",
        //                 tags: ["mongodb", "database", "infra", "customers-service", "live", "ready",]
        //             )
        //             .AddUrlGroup(
        //                 new List<Uri> { new($"{catalogsApiClientOptions.BaseApiAddress}/healthz") },
        //                 name: "Catalogs-Downstream-API-Check",
        //                 failureStatus: HealthStatus.Unhealthy,
        //                 timeout: TimeSpan.FromSeconds(3),
        //                 tags: ["uris", "downstream-services", "customers-service", "live", "ready",]
        //             )
        //             .AddUrlGroup(
        //                 new List<Uri> { new($"{identityApiClientOptions.BaseApiAddress}/healthz") },
        //                 name: "Identity-Downstream-API-Check",
        //                 failureStatus: HealthStatus.Unhealthy,
        //                 timeout: TimeSpan.FromSeconds(3),
        //                 tags: ["uris", "downstream-services", "customers-service", "live", "ready",]
        //             );
        //     });
        // }

        builder.Services.AddEmailService(builder.Configuration);

        builder.AddCompression();
        builder.AddAppProblemDetails();

        builder.AddCustomOpenTelemetry();

        builder.AddCustomSerilog();

        builder.AddCustomCors();

        builder.AddCustomVersioning();
        builder.AddCustomSwagger(cfg =>
        {
            cfg.Name = "Customers Apis";
            cfg.Title = "Customers Apis";
        });
        builder.Services.AddHttpContextAccessor();

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

        builder.Services.AddHeaderPropagation(options =>
        {
            options.HeaderNames.Add(MessageHeaders.CorrelationId);
        });

        builder.Services.AddPostgresMessagePersistence(builder.Configuration);

        // https://blog.maartenballiauw.be/post/2022/09/26/aspnet-core-rate-limiting-middleware.html
        builder.AddCustomRateLimit();

        builder.AddCustomMassTransit(
            configureMessagesTopologies: (context, cfg) =>
            {
                cfg.ConfigureUsersMessagesTopology(context);
                cfg.ConfigureProductMessagesTopology(context);

                cfg.ConfigureCustomerMessagesTopology();
                cfg.ConfigureRestockSubscriptionMessagesTopology();
            },
            configureMessagingOptions: msgCfg =>
            {
                msgCfg.AutoConfigEndpoints = false;
                msgCfg.OutboxEnabled = true;
                msgCfg.InboxEnabled = true;
            }
        );

        builder.Services.AddCustomValidators(Assembly.GetExecutingAssembly());

        builder.AddCustomEasyCaching();

        builder.AddCustomHttpClients();

        return builder;
    }
}
