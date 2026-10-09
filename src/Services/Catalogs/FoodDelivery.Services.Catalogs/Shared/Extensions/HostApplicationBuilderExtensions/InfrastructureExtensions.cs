using Squirrel.Caching.Behaviors;
using Squirrel.Caching.Extensions;
using Squirrel.Core.Constants;
using Squirrel.Core.Extensions;
using Squirrel.Core.Persistence.EfCore;
using Squirrel.Core.Pipelines;
using Squirrel.Core.Web.Extensions;
using Squirrel.Email;
using Squirrel.Integration.Wolverine.Extensions;
using Squirrel.OpenApi;
using Squirrel.OpenApi.AspnetOpenApi.Extensions;
using Squirrel.OpenApi.AsyncApi;
using Squirrel.SerilogLogging;
using Squirrel.SerilogLogging.Extensions;
using Squirrel.Validation;
using Squirrel.Validation.Extensions;
using Squirrel.Web.Cors;
using Squirrel.Web.Extensions;
using Squirrel.Web.Minimal.Extensions;
using Squirrel.Web.RateLimit;
using FoodDelivery.Services.Catalogs.Products;
using FoodDelivery.Services.Shared.Constants;
using Mediator;
using Microsoft.AspNetCore.HttpOverrides;

namespace FoodDelivery.Services.Catalogs.Shared.Extensions.HostApplicationBuilderExtensions;

public static partial class WebApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        // https://github.com/martinothamar/Mediator
        // if we have mediator we should register it before `AddCoreServices`; otherwise it uses NullMediator
        builder.Services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.Namespace = "FoodDelivery.Services.Catalogs";
        });

        builder.AddCoreServices();

        builder.AddCustomVersioning();
        builder.AddAspnetOpenApi(["v1", "v2"]);
        builder.AddAsyncApi([typeof(CatalogsMetadata)]);

        builder.AddDefaultCors();

        builder.AddCustomAuthentication();
        builder.AddCustomAuthorization();

        builder.AddWolverineEventBus(
            options =>
            {
                options.ConfigureProductPublishMessagesTopology();
            },
            configureWolverineBusOptions: msgCfg =>
            {
                msgCfg.AutoConfigMessagesTopology = false;
                msgCfg.EnableDurability = true;
            },
            durabilityConnectionStringName: AspireApplicationResources.PostgresDatabase.Catalogs,
            assemblies: [typeof(CatalogsMetadata).Assembly]
        );

        // register endpoints
        builder.AddMinimalEndpoints(typeof(CatalogsMetadata).Assembly);

        // https://blog.maartenballiauw.be/post/2022/09/26/aspnet-core-rate-limiting-middleware.html
        builder.AddCustomRateLimit();

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

        builder.Services.AddCustomValidators(typeof(CatalogsMetadata).Assembly);

        return builder;
    }
}
