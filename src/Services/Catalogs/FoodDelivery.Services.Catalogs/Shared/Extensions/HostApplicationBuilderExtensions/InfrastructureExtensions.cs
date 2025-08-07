using BuildingBlocks.Caching.Behaviors;
using BuildingBlocks.Caching.Extensions;
using BuildingBlocks.Core.Constants;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Persistence.EfCore;
using BuildingBlocks.Core.Pipelines;
using BuildingBlocks.Core.Web.Extensions;
using BuildingBlocks.Email;
using BuildingBlocks.Integration.MassTransit.Extensions;
using BuildingBlocks.Messaging.Persistence.Postgres;
using BuildingBlocks.OpenApi;
using BuildingBlocks.OpenApi.AspnetOpenApi.Extensions;
using BuildingBlocks.OpenApi.AsyncApi;
using BuildingBlocks.SerilogLogging;
using BuildingBlocks.SerilogLogging.Extensions;
using BuildingBlocks.Validation;
using BuildingBlocks.Validation.Extensions;
using BuildingBlocks.Web.Cors;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Minimal.Extensions;
using BuildingBlocks.Web.RateLimit;
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

        builder.AddMasstransitEventBus(
            (_, busFactoryConfigurator) =>
            {
                busFactoryConfigurator.ConfigureProductPublishMessagesTopology();
            },
            configureMasstransitOptions: msgCfg =>
            {
                msgCfg.AutoConfigMessagesTopology = false;
            },
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

        builder.AddPostgresMessagePersistence(
            connectionStringName: AspireApplicationResources.PostgresDatabase.Catalogs
        );

        return builder;
    }
}
