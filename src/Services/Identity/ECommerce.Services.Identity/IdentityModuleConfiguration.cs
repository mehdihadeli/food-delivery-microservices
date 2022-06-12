using BuildingBlocks.Abstractions.Web.Module;
using BuildingBlocks.Monitoring;
using ECommerce.Services.Identity.Identity;
using ECommerce.Services.Identity.Shared.Extensions.ApplicationBuilderExtensions;
using ECommerce.Services.Identity.Shared.Extensions.ServiceCollectionExtensions;
using ECommerce.Services.Identity.Users;

namespace ECommerce.Services.Identity;

public class IdentityModuleConfiguration : IRootModuleDefinition
{
    public const string IdentityModulePrefixUri = "api/v1/identity";

    public IServiceCollection AddModuleServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment)
    {
        services.AddInfrastructure(configuration, webHostEnvironment);

        // Add Sub Modules Services
        services.AddIdentityServices(configuration, webHostEnvironment);
        services.AddUsersServices(configuration);

        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", (HttpContext context) =>
        {
            var requestId = context.Request.Headers.TryGetValue("X-Request-Id", out var requestIdHeader)
                ? requestIdHeader.FirstOrDefault()
                : string.Empty;

            return $"Identity Service Apis, RequestId: {requestId}";
        }).ExcludeFromDescription();

        // Add Sub Modules Endpoints
        endpoints.MapIdentityEndpoints();
        endpoints.MapUsersEndpoints();

        return endpoints;
    }

    public async Task<WebApplication> ConfigureModule(WebApplication app)
    {
        if (app.Environment.IsEnvironment("test") == false)
        {
            app.UseMonitoring();
            app.UseIdentityServer();
        }

        await app.ApplyDatabaseMigrations(app.Logger);
        await app.SeedData(app.Logger, app.Environment);

        return app;
    }
}
