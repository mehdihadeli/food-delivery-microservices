using BuildingBlocks.Caching;
using BuildingBlocks.Caching.Behaviors;
using BuildingBlocks.Core.Diagnostics.Behaviors;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Persistence.EfCore;
using BuildingBlocks.Integration.MassTransit;
using BuildingBlocks.Messaging.Persistence.Postgres.Extensions;
using BuildingBlocks.OpenApi.AspnetOpenApi.Extensions;
using BuildingBlocks.SerilogLogging;
using BuildingBlocks.SerilogLogging.Extensions;
using BuildingBlocks.Validation;
using BuildingBlocks.Validation.Extensions;
using BuildingBlocks.Web.Cors;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Minimal.Extensions;
using BuildingBlocks.Web.RateLimit;
using FoodDelivery.Services.Orders.Customers;
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

        var serilogOptions = builder.Configuration.BindOptions<SerilogOptions>();
        if (serilogOptions.Enabled)
        {
            // - for production, we use OpenTelemetry
            // - we can use serilog to send logs to opentemetry with using`writeToProviders` and `builder.SeilogLogging.AddOpenTelemetry` to write logs event to `ILoggerProviders` which use by opentelemtry and .net default logging use it,
            // and here we used .net default logging without any configuration, and it is fully compatible with `builder.SeilogLogging.AddOpenTelemetry` for sending logs to opentelemetry
            builder.AddCustomSerilog();
        }

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

        builder.AddCustomCaching();

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

        builder.Services.AddPostgresMessagePersistence();

        return builder;
    }
}
