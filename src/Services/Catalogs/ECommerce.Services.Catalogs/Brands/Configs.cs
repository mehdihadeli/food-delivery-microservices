using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Abstractions.Web.Module;
using ECommerce.Services.Catalogs.Brands.Data;

namespace ECommerce.Services.Catalogs.Brands;

internal class Configs : IModuleConfiguration
{
    public WebApplicationBuilder AddModuleServices(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IDataSeeder, BrandDataSeeder>();

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
