using BuildingBlocks.Caching.Behaviors;
using BuildingBlocks.Caching.Extensions;
using BuildingBlocks.Core.Constants;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Persistence.EfCore;
using BuildingBlocks.Core.Pipelines;
using BuildingBlocks.Email;
using BuildingBlocks.Integration.MassTransit.Extensions;
using BuildingBlocks.Messaging.Persistence.Postgres;
using BuildingBlocks.OpenApi.AspnetOpenApi.Extensions;
using BuildingBlocks.SerilogLogging;
using BuildingBlocks.SerilogLogging.Extensions;
using BuildingBlocks.Validation;
using BuildingBlocks.Validation.Extensions;
using BuildingBlocks.Web.Cors;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Minimal.Extensions;
using BuildingBlocks.Web.RateLimit;
using FoodDelivery.Services.Customers.Customers;
using FoodDelivery.Services.Customers.Products;
using FoodDelivery.Services.Customers.RestockSubscriptions;
using FoodDelivery.Services.Customers.Shared.Clients.Rest.Catalogs.Rest;
using FoodDelivery.Services.Customers.Shared.Clients.Rest.Identity.Rest;
using FoodDelivery.Services.Customers.Users;
using FoodDelivery.Services.Shared.Constants;
using Mediator;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FoodDelivery.Services.Customers.Shared.Extensions.HostApplicationBuilderExtensions;

public static partial class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        // https://github.com/martinothamar/Mediator
        // if we have mediator we should register it before AddCoreServices; otherwise it uses NullMediator
        builder.Services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.Namespace = "FoodDelivery.Services.Customers";
        });

        builder.AddCoreServices();

        builder.AddCustomVersioning();
        builder.AddAspnetOpenApi(["v1", "v2"]);

        builder.AddDefaultCors();

        builder.AddCustomAuthentication();
        builder.AddCustomAuthorization();

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
            },
            assemblies: [typeof(CustomersMetadata).Assembly]
        );

        // register endpoints
        builder.AddMinimalEndpoints(typeof(CustomersMetadata).Assembly);

        // https://blog.maartenballiauw.be/post/2022/09/26/aspnet-core-rate-limiting-middleware.html
        builder.AddCustomRateLimit();

        var identityApiClientOptions = builder.Configuration.BindOptions<IdentityRestClientOptions>();
        var catalogsApiClientOptions = builder.Configuration.BindOptions<CatalogsRestClientOptions>();

        builder
            .Services.AddHealthChecks()
            .AddUrlGroup(
                new List<Uri> { new($"{catalogsApiClientOptions.BaseAddress}/healthz") },
                name: "Catalogs-Downstream-API-Check",
                failureStatus: HealthStatus.Unhealthy,
                timeout: TimeSpan.FromSeconds(3),
                tags: ["live"]
            )
            .AddUrlGroup(
                new List<Uri> { new($"{identityApiClientOptions.BaseAddress}/healthz") },
                name: "Identity-Downstream-API-Check",
                failureStatus: HealthStatus.Unhealthy,
                timeout: TimeSpan.FromSeconds(3),
                tags: ["live"]
            );

        builder.AddCustomCaching(redisConnectionStringName: AspireResources.Redis);

        builder.Services.AddEmailService(builder.Configuration);

        // https://github.com/tonerdo/dotnet-env
        DotNetEnv.Env.TraversePath().Load();

        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(StreamLoggingBehavior<,>));
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(StreamRequestValidationBehavior<,>));
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(EfTxBehavior<,>));
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(DiagnosticsPipelineBehavior<,>));
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(StreamCachingBehavior<,>));
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(InvalidateCachingBehavior<,>));

        builder.Services.AddCustomValidators(typeof(CustomersMetadata).Assembly);

        builder.AddPostgresMessagePersistence(
            connectionStringName: AspireApplicationResources.PostgresDatabase.Customers
        );

        // External Clients
        builder.AddCustomHttpClients();

        return builder;
    }
}
