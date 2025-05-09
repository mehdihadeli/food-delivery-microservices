using BuildingBlocks.Caching;
using BuildingBlocks.Caching.Behaviors;
using BuildingBlocks.Core.Diagnostics.Behaviors;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Messages;
using BuildingBlocks.Core.Persistence.EfCore;
using BuildingBlocks.Core.Security;
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
using FoodDelivery.Services.Shared;
using FoodDelivery.Services.Shared.Catalogs.Products;
using Mediator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RabbitMQ.Client;

namespace FoodDelivery.Services.Catalogs.Shared.Extensions.WebApplicationBuilderExtensions;

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
            options.Namespace = "FoodDelivery.Services.Catalogs";
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
            (busRegistrationContext, busFactoryConfigurator) =>
            {
                busFactoryConfigurator.ConfigureProductPublishMessagesTopology();
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

            postgresOptions.NotBeNull();
            rabbitMqOptions.NotBeNull();

            healthChecksBuilder
                .AddNpgSql(
                    postgresOptions.ConnectionString,
                    name: "CatalogsService-Postgres-Check",
                    tags: ["postgres", "infra", "database", "catalogs-service", "live", "ready"]
                )
                .AddRabbitMQ(
                    async sp =>
                    {
                        var factory = new ConnectionFactory { Uri = new Uri(rabbitMqOptions.ConnectionString) };
                        return await factory.CreateConnectionAsync().ConfigureAwait(false);
                    },
                    name: "CatalogsService-RabbitMQ-Check",
                    timeout: TimeSpan.FromSeconds(3),
                    tags: ["rabbitmq", "infra", "bus", "catalogs-service", "live", "ready"]
                );
        });

        builder.AddCustomResiliency(false);

        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/security
        var jwtOptions = builder.Configuration.BindOptions<JwtOptions>();
        builder
            .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = jwtOptions.Authority;
                options.Audience = jwtOptions.Audience;
                options.RequireHttpsMetadata = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = jwtOptions.ValidateIssuer,
                    ValidIssuers = jwtOptions.ValidIssuers,
                    ValidateAudience = jwtOptions.ValidateAudience,
                    ValidAudiences = jwtOptions.ValidAudiences,
                    ValidateLifetime = jwtOptions.ValidateLifetime,
                    ClockSkew = jwtOptions.ClockSkew,
                    // For IdentityServer4/Duende, we should also validate the signing key
                    ValidateIssuerSigningKey = true,
                    NameClaimType = "name", // Map "name" claim to User.Identity.Name
                    RoleClaimType = "role", // Map "role" claim to User.IsInRole()
                };

                // Preserve ALL claims from the token (including "sub")
                options.MapInboundClaims = false;
            });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(
                Permissions.CatalogsRead,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    // Check for client scope
                    policy.RequireClaim(ClaimsType.Scope, Scopes.CatalogsRead, Scopes.CatalogsFull);
                    // Check for user permission
                    policy.RequireClaim(ClaimsType.Permission, Permissions.CatalogsRead, Permissions.CatalogsFull);
                }
            );

            options.AddPolicy(
                Permissions.CatalogsWrite,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    // Check for client scope
                    policy.RequireClaim(ClaimsType.Scope, Scopes.CatalogsWrite, Scopes.CatalogsFull);
                    // Check for user permission
                    policy.RequireClaim(ClaimsType.Permission, Permissions.CatalogsWrite, Permissions.CatalogsFull);
                }
            );

            // Role-based policies
            options.AddPolicy(
                Role.Admin,
                x =>
                {
                    x.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    x.RequireRole(Role.Admin);
                }
            );
            options.AddPolicy(
                Role.User,
                x =>
                {
                    x.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    x.RequireRole(Role.User);
                }
            );
        });

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

        builder.Services.AddCustomValidators(typeof(CatalogsMetadata).Assembly);

        builder.Services.AddPostgresMessagePersistence();

        return builder;
    }
}
