using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Abstractions.Web.Module;
using ECommerce.Services.Catalogs.Suppliers.Data;

namespace ECommerce.Services.Catalogs.Suppliers;

internal class Configs : IModuleConfiguration
{
    public WebApplicationBuilder AddModuleServices(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IDataSeeder, SupplierDataSeeder>();

        return builder;
    }

    public Task<WebApplication> ConfigureModule(WebApplication app)
    {
        return Task.FromResult(app);
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        return endpoints;
    }
}
