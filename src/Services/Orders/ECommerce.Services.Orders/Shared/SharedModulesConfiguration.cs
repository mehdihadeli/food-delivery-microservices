using BuildingBlocks.Abstractions.Web.Module;
using BuildingBlocks.Core;
using ECommerce.Services.Orders.Shared.Extensions.WebApplicationBuilderExtensions;
using ECommerce.Services.Orders.Shared.Extensions.WebApplicationExtensions;

namespace ECommerce.Services.Orders.Shared;

public class SharedModulesConfiguration : ISharedModulesConfiguration
{
    public const string OrderModulePrefixUri = "api/v{version:apiVersion}/orders";

    public WebApplicationBuilder AddSharedModuleServices(WebApplicationBuilder builder)
    {
        builder.AddInfrastructure();

        builder.AddStorage();

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

            return $"Orders Service Apis, RequestId: {requestId}";
        }).ExcludeFromDescription();

        return endpoints;
    }
}
