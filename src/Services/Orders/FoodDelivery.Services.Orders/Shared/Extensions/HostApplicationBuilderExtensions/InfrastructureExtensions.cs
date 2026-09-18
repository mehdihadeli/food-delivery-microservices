using CloudNativeKit.Caching.Behaviors;
using CloudNativeKit.Caching.Extensions;
using CloudNativeKit.Core.Constants;
using CloudNativeKit.Core.Extensions;
using CloudNativeKit.Core.Persistence.EfCore;
using CloudNativeKit.Core.Pipelines;
using CloudNativeKit.Integration.Wolverine.Extensions;
using CloudNativeKit.OpenApi.AspnetOpenApi.Extensions;
using CloudNativeKit.Validation;
using CloudNativeKit.Validation.Extensions;
using CloudNativeKit.Web.Cors;
using CloudNativeKit.Web.Extensions;
using CloudNativeKit.Web.Minimal.Extensions;
using CloudNativeKit.Web.RateLimit;
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

        builder.AddWolverineEventBus(
            configureMessagesTopologies: options =>
            {
                options.AddCustomerEndpoints();
            },
            configureWolverineBusOptions: msgCfg =>
            {
                msgCfg.AutoConfigMessagesTopology = false;
                msgCfg.EnableDurability = true;
            },
            durabilityConnectionStringName: AspireApplicationResources.PostgresDatabase.Orders,
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

        return builder;
    }
}
