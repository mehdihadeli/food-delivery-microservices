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
using BuildingBlocks.Web.Extensions;
using ECommerce.Services.Customers.Customers;
using ECommerce.Services.Customers.Products;
using ECommerce.Services.Customers.RestockSubscriptions;
using ECommerce.Services.Customers.Shared.Clients.Catalogs;
using ECommerce.Services.Customers.Shared.Clients.Identity;
using ECommerce.Services.Customers.Users;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog.Events;

namespace ECommerce.Services.Customers.Shared.Extensions.WebApplicationBuilderExtensions;

public static partial class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        SnowFlakIdGenerator.Configure(2);

        builder.Services.AddCore(builder.Configuration);

        builder.Services.AddCustomJwtAuthentication(builder.Configuration);
        builder.Services.AddCustomAuthorization(
            rolePolicies: new List<RolePolicy>
            {
                new(CustomersConstants.Role.Admin, new List<string> {CustomersConstants.Role.Admin}),
                new(CustomersConstants.Role.User, new List<string> {CustomersConstants.Role.User})
            });

        // https://www.michaco.net/blog/EnvironmentVariablesAndConfigurationInASPNETCoreApps#environment-variables-and-configuration
        // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0#non-prefixed-environment-variables
        builder.Configuration.AddEnvironmentVariables("ecommerce_customers_env_");

        // https://github.com/tonerdo/dotnet-env
        DotNetEnv.Env.TraversePath().Load();

        if (builder.Environment.IsTest() == false)
        {
            builder.Services.AddCustomHealthCheck(healthChecksBuilder =>
            {
                var postgresOptions = builder.Configuration.GetOptions<PostgresOptions>();
                var rabbitMqOptions = builder.Configuration.GetOptions<RabbitMqOptions>();
                var mongoOptions = builder.Configuration.GetOptions<MongoOptions>();
                var identityApiClientOptions =
                    builder.Configuration.GetOptions<IdentityApiClientOptions>("IdentityApiClientOptions");
                var catalogsApiClientOptions = builder.Configuration.GetOptions<CatalogsApiClientOptions>();

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
        }

        builder.Services.AddEmailService(builder.Configuration);

        builder.AddCompression();
        builder.AddCustomProblemDetails();

        builder.AddCustomOpenTelemetry();

        builder.AddCustomSerilog(
            optionsBuilder =>
            {
                optionsBuilder.SetLevel(LogEventLevel.Information);
            });

        builder.AddCustomVersioning();
        builder.AddCustomSwagger(typeof(CustomersRoot).Assembly);
        builder.Services.AddHttpContextAccessor();

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
