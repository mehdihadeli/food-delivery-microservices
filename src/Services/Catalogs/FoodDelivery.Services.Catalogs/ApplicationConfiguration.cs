using FoodDelivery.Services.Catalogs.Brands;
using FoodDelivery.Services.Catalogs.Categories;
using FoodDelivery.Services.Catalogs.Products;
using FoodDelivery.Services.Catalogs.Shared.Extensions.HostApplicationBuilderExtensions;
using FoodDelivery.Services.Catalogs.Suppliers;

namespace FoodDelivery.Services.Catalogs;

public static class ApplicationConfiguration
{
    public const string CatalogModulePrefixUri = "api/v{version:apiVersion}";

    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.AddStorage();

        // Modules
        builder.AddProductsModuleServices();
        builder.AddBrandsModuleServices();
        builder.AddCategoriesModuleServices();
        builder.AddSuppliersModuleServices();

        return builder;
    }

    public static IEndpointRouteBuilder MapApplicationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", (HttpContext context) => "Catalogs Service Api.").ExcludeFromDescription();

        // Modules
        endpoints.MapProductsModuleEndpoints();

        return endpoints;
    }
}
