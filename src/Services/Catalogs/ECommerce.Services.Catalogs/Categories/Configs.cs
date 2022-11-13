using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Abstractions.Web.Module;
using ECommerce.Services.Catalogs.Categories.Data;

namespace ECommerce.Services.Catalogs.Categories;

internal class Configs : IModuleConfiguration
{
    public WebApplicationBuilder AddModuleServices(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IDataSeeder, CategoryDataSeeder>();

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
