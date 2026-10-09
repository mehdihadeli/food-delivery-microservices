using Squirrel.Caching.Behaviors;
using Squirrel.Caching.Extensions;
using Squirrel.Core.Constants;
using Squirrel.Core.Extensions;
using Squirrel.Core.Persistence.EfCore;
using Squirrel.Core.Pipelines;
using Squirrel.Email;
using Squirrel.Integration.Wolverine.Extensions;
using Squirrel.OpenApi.AspnetOpenApi.Extensions;
using Squirrel.SerilogLogging;
using Squirrel.Validation;
using Squirrel.Validation.Extensions;
using Squirrel.Web.Cors;
using Squirrel.Web.Extensions;
using Squirrel.Web.Minimal.Extensions;
using Squirrel.Web.RateLimit;
using FoodDelivery.Services.Identity.Users;
using FoodDelivery.Services.Shared.Constants;
using Mediator;

namespace FoodDelivery.Services.Identity.Shared.Extensions.HostApplicationBuilderExtensions;

public static partial class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        // https://github.com/martinothamar/Mediator
        // if we have mediator we should register it before AddCoreServices; otherwise it uses NullMediator
        builder.Services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.Namespace = "FoodDelivery.Services.Identity";
        });

        builder.AddCoreServices();

        // for identity server ui
        builder.Services.AddRazorPages();

        builder.AddCustomVersioning();
        builder.AddAspnetOpenApi(["v1", "v2"]);

        builder.AddDefaultCors();

        builder.AddCustomAuthentication();
        builder.AddCustomAuthorization();

        builder.AddWolverineEventBus(
            configureMessagesTopologies: options =>
            {
                options.ConfigureUserPublishMessagesTopology();
            },
            configureWolverineBusOptions: msgCfg =>
            {
                msgCfg.AutoConfigMessagesTopology = false;
                msgCfg.EnableDurability = true;
            },
            durabilityConnectionStringName: AspireApplicationResources.PostgresDatabase.Identity,
            assemblies: [typeof(IdentityMetadata).Assembly]
        );

        // https://blog.maartenballiauw.be/post/2022/09/26/aspnet-core-rate-limiting-middleware.html
        builder.AddCustomRateLimit();

        builder.AddCustomCaching(redisConnectionStringName: AspireResources.Redis);

        builder.Services.AddEmailService(builder.Configuration);

        // https://github.com/tonerdo/dotnet-env
        DotNetEnv.Env.TraversePath().Load();

        // register endpoints
        builder.AddMinimalEndpoints(typeof(IdentityMetadata).Assembly);

        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(StreamLoggingBehavior<,>));
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(StreamRequestValidationBehavior<,>));
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(EfTxBehavior<,>));
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(DiagnosticsPipelineBehavior<,>));
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(StreamCachingBehavior<,>));
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(InvalidateCachingBehavior<,>));

        builder.Services.AddCustomValidators(typeof(IdentityMetadata).Assembly);

        builder.AddCustomIdentity();

        builder.AddCustomIdentityServer();

        return builder;
    }
}
