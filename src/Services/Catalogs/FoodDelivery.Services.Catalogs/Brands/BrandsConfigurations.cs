using FoodDelivery.Services.Catalogs.Brands.Contracts;
using FoodDelivery.Services.Catalogs.Brands.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FoodDelivery.Services.Catalogs.Brands;

internal static class BrandsConfigurations
{
    internal static WebApplicationBuilder AddBrandsModuleServices(this WebApplicationBuilder builder)
    {
        builder.Services.TryAddScoped<IBrandChecker, BrandChecker>();

        return builder;
    }
}
