using FoodDelivery.Services.Catalogs.Brands;
using FoodDelivery.Services.Catalogs.Categories;
using FoodDelivery.Services.Catalogs.Products;
using FoodDelivery.Services.Catalogs.Shared.Extensions.WebApplicationBuilderExtensions;
using FoodDelivery.Services.Catalogs.Suppliers;

namespace FoodDelivery.Services.Catalogs;

public static class CatalogsConfigurations
{
    public const string CatalogModulePrefixUri = "api/v{version:apiVersion}/catalogs";

    public static WebApplicationBuilder AddCatalogsServices(this WebApplicationBuilder builder)
    {
        // Catalogs Configurations
        builder.AddStorage();

        // Modules
        builder.AddProductsModuleServices();
        builder.AddBrandsModuleServices();
        builder.AddCategoriesModuleServices();
        builder.AddSuppliersModuleServices();

        return builder;
    }

    public static WebApplication UseCatalogs(this WebApplication app)
    {
        // Modules
        app.UseProductsModule();
        app.UseBrandsModule();
        app.UseCategoriesModule();
        app.UseSuppliersModule();

        return app;
    }

    public static IEndpointRouteBuilder MapCatalogsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", (HttpContext context) => "Catalogs Service Api.").ExcludeFromDescription();

        // Modules
        endpoints.MapProductsModuleEndpoints();

        return endpoints;
    }
}
