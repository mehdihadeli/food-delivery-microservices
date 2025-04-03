using FoodDelivery.Services.Catalogs.Categories.Contracts;
using FoodDelivery.Services.Catalogs.Categories.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FoodDelivery.Services.Catalogs.Categories;

internal static class CategoriesConfigurations
{
    internal static WebApplicationBuilder AddCategoriesModuleServices(this WebApplicationBuilder builder)
    {
        builder.Services.TryAddScoped<ICategoryChecker, CategoryChecker>();

        return builder;
    }

    internal static WebApplication UseCategoriesModule(this WebApplication app)
    {
        return app;
    }
}
