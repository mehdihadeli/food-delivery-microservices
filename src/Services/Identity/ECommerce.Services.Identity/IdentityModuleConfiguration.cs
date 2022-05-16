using System.Reflection;
using Ardalis.GuardClauses;
using BuildingBlocks.Caching.InMemory;
using BuildingBlocks.Core.Caching;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Persistence.EfCore;
using BuildingBlocks.Core.Registrations;
using BuildingBlocks.Email;
using BuildingBlocks.Integration.MassTransit;
using BuildingBlocks.Logging;
using BuildingBlocks.Monitoring;
using BuildingBlocks.Persistence.EfCore.Postgres;
using BuildingBlocks.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ECommerce.Services.Identity.Identity;
using ECommerce.Services.Identity.Shared.Extensions.ApplicationBuilderExtensions;
using ECommerce.Services.Identity.Users;

namespace ECommerce.Services.Identity;

public static class IdentityModuleConfiguration
{
    public const string IdentityModulePrefixUri = "api/v1/identity";

    public static WebApplicationBuilder AddIdentityModule(this WebApplicationBuilder builder)
    {
        AddIdentityModuleServices(builder.Services, builder.Configuration);

        return builder;
    }

    public static IServiceCollection AddIdentityModuleServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCore(configuration);
        services.AddEmailService(configuration);

        services.AddCustomValidators(Assembly.GetExecutingAssembly());
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddCqrs(doMoreActions: s =>
        {
            s.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>))
                .AddScoped(typeof(IStreamPipelineBehavior<,>), typeof(StreamRequestValidationBehavior<,>))
                .AddScoped(typeof(IStreamPipelineBehavior<,>), typeof(StreamLoggingBehavior<,>))
                .AddScoped(typeof(IStreamPipelineBehavior<,>), typeof(StreamCachingBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(InvalidateCachingBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(EfTxBehavior<,>));
        });

        services.AddInMemoryMessagePersistence();
        services.AddCustomMassTransit(
            configuration,
            (context, cfg) =>
            {
                cfg.AddUserPublishers();
            },
            autoConfigEndpoints: false);

        services.AddMonitoring(healthChecksBuilder =>
        {
            var postgresOptions = configuration.GetOptions<PostgresOptions>(nameof(PostgresOptions));
            Guard.Against.Null(postgresOptions, nameof(postgresOptions));

            healthChecksBuilder.AddNpgSql(
                postgresOptions.ConnectionString,
                name: "Identity-Postgres-Check",
                tags: new[] {"identity-postgres"});
        });

        services.AddCustomInMemoryCache(configuration)
            .AddCachingRequestPolicies(Assembly.GetExecutingAssembly());

        services.AddIdentityServices(configuration);

        services.AddUsersServices(configuration);

        return services;
    }

    public static async Task ConfigureIdentityModule(
        this IApplicationBuilder app,
        IWebHostEnvironment environment,
        ILogger logger)
    {
        app.UseMonitoring();

        app.UseIdentityServer();

        await app.ApplyDatabaseMigrations(logger);
        await app.SeedData(logger, environment);
    }

    public static IEndpointRouteBuilder MapIdentityModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", (HttpContext context) =>
        {
            var requestId = context.Request.Headers.TryGetValue("X-Request-Id", out var requestIdHeader)
                ? requestIdHeader.FirstOrDefault()
                : string.Empty;

            return $"Identity Service Apis, RequestId: {requestId}";
        }).ExcludeFromDescription();

        endpoints.MapIdentityEndpoints();
        endpoints.MapUsersEndpoints();

        return endpoints;
    }
}
