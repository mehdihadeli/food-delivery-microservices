using BuildingBlocks.Caching;
using BuildingBlocks.Caching.Behaviors;
using BuildingBlocks.Core.Diagnostics.Behaviors;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Messages;
using BuildingBlocks.Core.Persistence.EfCore;
using BuildingBlocks.Core.Web.Extensions;
using BuildingBlocks.Email;
using BuildingBlocks.HealthCheck;
using BuildingBlocks.Integration.MassTransit;
using BuildingBlocks.Messaging.Persistence.Postgres.Extensions;
using BuildingBlocks.Observability.Extensions;
using BuildingBlocks.Persistence.EfCore.Postgres;
using BuildingBlocks.Persistence.Mongo;
using BuildingBlocks.Resiliency;
using BuildingBlocks.Security.Jwt;
using BuildingBlocks.SerilogLogging;
using BuildingBlocks.SerilogLogging.Extensions;
using BuildingBlocks.Validation;
using BuildingBlocks.Validation.Extensions;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Extensions.WebApplicationBuilderExtensions;
using BuildingBlocks.Web.RateLimit;
using FoodDelivery.Services.Customers.Customers;
using FoodDelivery.Services.Customers.Products;
using FoodDelivery.Services.Customers.RestockSubscriptions;
using FoodDelivery.Services.Customers.Shared.Clients.Rest.Catalogs.Rest;
using FoodDelivery.Services.Customers.Shared.Clients.Rest.Identity.Rest;
using FoodDelivery.Services.Customers.Users;
using FoodDelivery.Services.Shared.Customers.Customers;
using FoodDelivery.Services.Shared.Customers.Products;
using FoodDelivery.Services.Shared.Customers.RestockSubscriptions;
using FoodDelivery.Services.Shared.Customers.Users;
using Mediator;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;
using RabbitMQ.Client;

namespace FoodDelivery.Services.Customers.Shared.Extensions.WebApplicationBuilderExtensions;

public static partial class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        builder.AddAppProblemDetails();

        // https://github.com/martinothamar/Mediator
        // if we have mediator we should register it before AddCore, otherwise it uses NullMediator
        builder.Services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.Namespace = "FoodDelivery.Services.Customers";
        });

        builder.AddCore();

        var serilogOptions = builder.Configuration.BindOptions<SerilogOptions>(nameof(SerilogOptions));
        if (serilogOptions.Enabled && (builder.Environment.IsDevelopment() || builder.Environment.IsTest()))
        {
            // - for production, we use OpenTelemetry
            // - we can use serilog to send logs to opentemetry with using`writeToProviders` and `builder.SeilogLogging.AddOpenTelemetry` to write logs event to `ILoggerProviders` which use by opentelemtry and .net default logging use it,
            // and here we used .net default logging without any configuration, and it is fully compatible with `builder.SeilogLogging.AddOpenTelemetry` for sending logs to opentelemetry
            builder.AddCustomSerilog();
        }

        // https://aurelien-riv.github.io/aspnetcore/2022/11/09/aspnet-grafana-loki-telemetry-microservice-correlation.html
        // https://www.nuget.org/packages/Microsoft.AspNetCore.HeaderPropagation
        // https://gist.github.com/davidfowl/c34633f1ddc519f030a1c0c5abe8e867
        // https://github.com/dotnet/aspnetcore/blob/main/src/Middleware/HeaderPropagation/test/HeaderPropagationIntegrationTest.cs
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-9.0
        builder.Services.AddHeaderPropagation(options =>
        {
            options.Headers.Add(MessageHeaders.CorrelationId);
            options.Headers.Add(MessageHeaders.CausationId);
        });

        builder.AddMasstransitEventBus(
            configureMessagesTopologies: (context, cfg) =>
            {
                cfg.ConfigureUsersConsumeMessagesTopology(context);
                cfg.ConfigureProductsConsumeMessagesTopology(context);

                cfg.ConfigureCustomerPublishMessagesTopology();
                cfg.ConfigureRestockSubscriptionPublishMessagesTopology();
            },
            configureMasstransitOptions: msgCfg =>
            {
                msgCfg.AutoConfigMessagesTopology = false;
            }
        );

        // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/health-checks#non-development-environments
        builder.Services.AddRequestTimeouts();
        builder.Services.AddOutputCache();

        builder.AddCustomObservability();

        // https://blog.maartenballiauw.be/post/2022/09/26/aspnet-core-rate-limiting-middleware.html
        builder.AddCustomRateLimit();

        builder.AddCustomHealthCheck(healthChecksBuilder =>
        {
            var postgresOptions = builder.Configuration.BindOptions<PostgresOptions>(nameof(PostgresOptions));
            var rabbitMqOptions = builder.Configuration.BindOptions<RabbitMqOptions>(nameof(RabbitMqOptions));
            var mongoOptions = builder.Configuration.BindOptions<MongoOptions>();
            var identityApiClientOptions = builder.Configuration.BindOptions<IdentityRestClientOptions>();
            var catalogsApiClientOptions = builder.Configuration.BindOptions<CatalogsRestClientOptions>();

            postgresOptions.NotBeNull();
            rabbitMqOptions.NotBeNull();
            mongoOptions.NotBeNull();
            identityApiClientOptions.NotBeNull();
            catalogsApiClientOptions.NotBeNull();

            healthChecksBuilder
                .AddNpgSql(
                    postgresOptions.ConnectionString,
                    name: "CustomersService-Postgres-Check",
                    tags: ["postgres", "infra", "database", "catalogs-service", "live", "ready"]
                )
                .AddRabbitMQ(
                    async sp =>
                    {
                        var factory = new ConnectionFactory { Uri = new Uri(rabbitMqOptions.ConnectionString) };
                        return await factory.CreateConnectionAsync().ConfigureAwait(false);
                    },
                    name: "CustomersService-RabbitMQ-Check",
                    timeout: TimeSpan.FromSeconds(3),
                    tags: ["rabbitmq", "infra", "bus", "catalogs-service", "live", "ready"]
                )
                .AddMongoDb(
                    dbFactory: sp => sp.GetRequiredService<IMongoDatabase>(),
                    name: "CustomersService-Mongo-Check",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: ["mongodb", "database", "infra", "customers-service", "live", "ready"],
                    timeout: TimeSpan.FromSeconds(5)
                )
                .AddUrlGroup(
                    new List<Uri> { new($"{catalogsApiClientOptions.BaseAddress}/healthz") },
                    name: "Catalogs-Downstream-API-Check",
                    failureStatus: HealthStatus.Unhealthy,
                    timeout: TimeSpan.FromSeconds(3),
                    tags: ["uris", "downstream-services", "customers-service", "live", "ready"]
                )
                .AddUrlGroup(
                    new List<Uri> { new($"{identityApiClientOptions.BaseAddress}/healthz") },
                    name: "Identity-Downstream-API-Check",
                    failureStatus: HealthStatus.Unhealthy,
                    timeout: TimeSpan.FromSeconds(3),
                    tags: ["uris", "downstream-services", "customers-service", "live", "ready"]
                );
        });

        builder.AddCustomResiliency(false);

        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/security
        builder.Services.AddCustomJwtAuthentication(builder.Configuration);
        builder.Services.AddCustomAuthorization(
            rolePolicies: new List<RolePolicy>
            {
                new(CustomersConstants.Role.Admin, new List<string> { CustomersConstants.Role.Admin }),
                new(CustomersConstants.Role.User, new List<string> { CustomersConstants.Role.User }),
            }
        );

        builder.AddCustomCaching();

        builder.Services.AddEmailService(builder.Configuration);

        builder.AddCustomVersioning();

        builder.AddCustomCors();

        // https://github.com/tonerdo/dotnet-env
        DotNetEnv.Env.TraversePath().Load();

        builder.AddCompression();

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(StreamLoggingBehavior<,>));
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(StreamRequestValidationBehavior<,>));
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(EfTxBehavior<,>));
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ObservabilityPipelineBehavior<,>));
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(StreamCachingBehavior<,>));
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(InvalidateCachingBehavior<,>));

        builder.Services.AddCustomValidators(typeof(CustomersMetadata).Assembly);

        builder.Services.AddPostgresMessagePersistence(builder.Configuration);

        // External Clients
        builder.AddCustomHttpClients();

        return builder;
    }
}
