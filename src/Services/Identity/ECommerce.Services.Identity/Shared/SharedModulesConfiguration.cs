using BuildingBlocks.Abstractions.Web.Module;
using BuildingBlocks.Core;
using ECommerce.Services.Identity.Shared.Extensions.WebApplicationBuilderExtensions;
using ECommerce.Services.Identity.Shared.Extensions.WebApplicationExtensions;

namespace ECommerce.Services.Identity.Shared;

public class SharedModulesConfiguration : ISharedModulesConfiguration
{
    public const string IdentityModulePrefixUri = "api/v{version:apiVersion}/identity";

    public WebApplicationBuilder AddSharedModuleServices(WebApplicationBuilder builder)
    {
        builder.AddInfrastructure();
        return builder;
    }

    public async Task<WebApplication> ConfigureSharedModule(WebApplication app)
    {
        await app.UseInfrastructure();

        ServiceActivator.Configure(app.Services);

        await app.ApplyDatabaseMigrations();
        await app.SeedData();

        return app;
    }

    public IEndpointRouteBuilder MapSharedModuleEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", (HttpContext context) =>
        {
            var requestId = context.Request.Headers.TryGetValue("X-Request-InternalCommandId", out var requestIdHeader)
                ? requestIdHeader.FirstOrDefault()
                : string.Empty;

            return $"Identity Service Apis, RequestId: {requestId}";
        }).ExcludeFromDescription();

        return endpoints;
    }
}
