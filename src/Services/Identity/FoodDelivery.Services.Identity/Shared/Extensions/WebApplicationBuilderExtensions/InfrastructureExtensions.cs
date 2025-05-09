using BuildingBlocks.Caching;
using BuildingBlocks.Caching.Behaviors;
using BuildingBlocks.Core.Diagnostics.Behaviors;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollectionExtensions;
using BuildingBlocks.Core.Messages;
using BuildingBlocks.Core.Persistence.EfCore;
using BuildingBlocks.Core.Web.Extensions;
using BuildingBlocks.Email;
using BuildingBlocks.HealthCheck;
using BuildingBlocks.Integration.MassTransit;
using BuildingBlocks.Messaging.Persistence.Postgres.Extensions;
using BuildingBlocks.Observability.Extensions;
using BuildingBlocks.Persistence.EfCore.Postgres;
using BuildingBlocks.Resiliency;
using BuildingBlocks.Security.Jwt;
using BuildingBlocks.SerilogLogging;
using BuildingBlocks.SerilogLogging.Extensions;
using BuildingBlocks.Validation;
using BuildingBlocks.Validation.Extensions;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Extensions.WebApplicationBuilderExtensions;
using BuildingBlocks.Web.RateLimit;
using FoodDelivery.Services.Identity.Shared.Data;
using FoodDelivery.Services.Identity.Shared.Models;
using FoodDelivery.Services.Identity.Users;
using FoodDelivery.Services.Shared.Identity.Users;
using Mediator;
using Microsoft.AspNetCore.Identity;
using RabbitMQ.Client;

namespace FoodDelivery.Services.Identity.Shared.Extensions.WebApplicationBuilderExtensions;

public static partial class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        builder.AddAppProblemDetails();

        builder.Services.AddAuthentication();

        // for identity server ui
        builder.Services.AddRazorPages();

        // https://github.com/martinothamar/Mediator
        // if we have mediator we should register it before AddCore, otherwise it uses NullMediator
        builder.Services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.Namespace = "FoodDelivery.Services.Identity";
        });

        builder.AddCore();

        var serilogOptions = builder.Configuration.BindOptions<SerilogOptions>(nameof(SerilogOptions));
        if (serilogOptions.Enabled)
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
        builder.Services.AddHeaderPropagation(options =>
        {
            options.Headers.Add(MessageHeaders.CorrelationId);
            options.Headers.Add(MessageHeaders.CausationId);
        });

        builder.AddMasstransitEventBus(
            configureMessagesTopologies: (context, cfg) =>
            {
                cfg.ConfigureUserPublishMessagesTopology();
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
            var postgresOptions = builder.Configuration.BindOptions<PostgresOptions>();
            var rabbitMqOptions = builder.Configuration.BindOptions<RabbitMqOptions>();

            healthChecksBuilder
                .AddNpgSql(
                    postgresOptions.ConnectionString,
                    name: "IdentityService-Postgres-Check",
                    tags: ["postgres", "database", "infra", "identity-service", "live", "ready"]
                )
                .AddRabbitMQ(
                    async sp =>
                    {
                        var factory = new ConnectionFactory { Uri = new Uri(rabbitMqOptions.ConnectionString) };

                        return await factory.CreateConnectionAsync().ConfigureAwait(false);
                    },
                    name: "IdentityService-RabbitMQ-Check",
                    timeout: TimeSpan.FromSeconds(3),
                    tags: ["rabbitmq", "infra", "bus", "catalogs-service", "live", "ready"]
                );
        });

        builder.AddCustomResiliency(false);

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

        builder.Services.AddCustomValidators(typeof(IdentityMetadata).Assembly);

        builder.Services.AddPostgresMessagePersistence();

        builder.AddCustomIdentity();

        if (builder.Environment.IsTest() == false)
            builder.AddCustomIdentityServer();

        return builder;
    }
}
