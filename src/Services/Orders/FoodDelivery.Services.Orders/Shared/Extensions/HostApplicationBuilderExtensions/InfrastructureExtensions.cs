using BuildingBlocks.Caching.Behaviors;
using BuildingBlocks.Caching.Extensions;
using BuildingBlocks.Core.Constants;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Persistence.EfCore;
using BuildingBlocks.Core.Pipelines;
using BuildingBlocks.Integration.MassTransit.Extensions;
using BuildingBlocks.Messaging.Persistence.Postgres;
using BuildingBlocks.OpenApi.AspnetOpenApi.Extensions;
using BuildingBlocks.Validation;
using BuildingBlocks.Validation.Extensions;
using BuildingBlocks.Web.Cors;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Minimal.Extensions;
using BuildingBlocks.Web.RateLimit;
using FoodDelivery.Services.Orders.Customers;
using FoodDelivery.Services.Shared.Constants;
using Mediator;

namespace FoodDelivery.Services.Orders.Shared.Extensions.HostApplicationBuilderExtensions;

public static partial class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        // https://github.com/martinothamar/Mediator
        // if we have mediator we should register it before AddCoreServices; otherwise it uses NullMediator
        builder.Services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.Namespace = "FoodDelivery.Services.Orders";
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
                cfg.AddCustomerEndpoints(context);
            },
            configureMasstransitOptions: msgCfg =>
            {
                msgCfg.AutoConfigMessagesTopology = false;
            },
            assemblies: [typeof(OrdersMetadata).Assembly]
        );

        // register endpoints
        builder.AddMinimalEndpoints(typeof(OrdersMetadata).Assembly);

        // https://blog.maartenballiauw.be/post/2022/09/26/aspnet-core-rate-limiting-middleware.html
        builder.AddCustomRateLimit();

        builder.AddCustomCaching(redisConnectionStringName: AspireResources.Redis);

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

        builder.Services.AddCustomValidators(typeof(OrdersMetadata).Assembly);

        builder.AddPostgresMessagePersistence(connectionStringName: AspireApplicationResources.PostgresDatabase.Orders);

        return builder;
    }
}
