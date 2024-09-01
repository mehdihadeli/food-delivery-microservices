using BuildingBlocks.Abstractions.Web.Module;
using FoodDelivery.Services.Catalogs.Suppliers.Contracts;
using FoodDelivery.Services.Catalogs.Suppliers.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FoodDelivery.Services.Catalogs.Suppliers;

internal class Configs : IModuleConfiguration
{
    public WebApplicationBuilder AddModuleServices(WebApplicationBuilder builder)
    {
        builder.Services.TryAddScoped<ISupplierChecker, SupplierChecker>();

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
