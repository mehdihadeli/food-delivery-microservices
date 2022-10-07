using BuildingBlocks.Abstractions.Web.Module;
using BuildingBlocks.Monitoring;
using ECommerce.Services.Identity.Shared.Extensions.ApplicationBuilderExtensions;
using ECommerce.Services.Identity.Shared.Extensions.ServiceCollectionExtensions;

namespace ECommerce.Services.Identity.Shared;

public class IdentityModuleConfiguration : ISharedModulesConfiguration
{
    public const string IdentityModulePrefixUri = "api/v1/identity";

    public IServiceCollection AddSharedModuleServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment)
    {
        services.AddInfrastructure(configuration, webHostEnvironment);

        return services;
    }

    public async Task<WebApplication> ConfigureSharedModule(WebApplication app)
    {
        if (app.Environment.IsEnvironment("test") == false)
        {
            app.UseMonitoring();
            app.UseIdentityServer();
        }

        await app.ApplyDatabaseMigrations(app.Logger);
        await app.SeedData(app.Logger, app.Environment);

        await app.UseInfrastructure(app.Logger);

        return app;
    }

    public IEndpointRouteBuilder MapSharedModuleEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", (HttpContext context) =>
        {
            var requestId = context.Request.Headers.TryGetValue("X-Request-Id", out var requestIdHeader)
                ? requestIdHeader.FirstOrDefault()
                : string.Empty;

            return $"Identity Service Apis, RequestId: {requestId}";
        }).ExcludeFromDescription();

        return endpoints;
    }
}
