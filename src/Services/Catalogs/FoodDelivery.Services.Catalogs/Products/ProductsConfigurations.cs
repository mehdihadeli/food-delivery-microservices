using FoodDelivery.Services.Catalogs.Products.Features.CreatingProduct.v1;
using FoodDelivery.Services.Catalogs.Products.Features.DebitingProductStock.v1;
using FoodDelivery.Services.Catalogs.Products.Features.GettingProductById.v1;
using FoodDelivery.Services.Catalogs.Products.Features.GettingProducts.v1;
using FoodDelivery.Services.Catalogs.Products.Features.GettingProductsView.v1;
using FoodDelivery.Services.Catalogs.Products.Features.ReplenishingProductStock.v1;
using FoodDelivery.Services.Catalogs.Products.Features.UpdatingProduct.v1;

namespace FoodDelivery.Services.Catalogs.Products;

internal static class ProductsConfigurations
{
    public const string Tag = "Products";
    public const string ProductsPrefixUri = $"{ApplicationConfiguration.CatalogModulePrefixUri}/products";

    internal static WebApplicationBuilder AddProductsModuleServices(this WebApplicationBuilder builder)
    {
        return builder;
    }

    internal static IEndpointRouteBuilder MapProductsModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // https://github.com/dotnet/aspnet-api-versioning/commit/b789e7e980e83a7d2f82ce3b75235dee5e0724b4
        // changed from MapApiGroup to NewVersionedApi in v7.0.0
        var routeCategoryName = Tag;
        var products = endpoints.NewVersionedApi(name: routeCategoryName).WithTags(Tag);

        // create a new subgroup for each version
        var productsV1 = products.MapGroup(ProductsPrefixUri).HasDeprecatedApiVersion(0.9).HasApiVersion(1.0);

        // create a new subgroup for each version
        var productsV2 = products.MapGroup(ProductsPrefixUri).HasApiVersion(2.0);

        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-7.0#route-groups
        // https://github.com/dotnet/aspnet-api-versioning/blob/main/examples/AspNetCore/WebApi/MinimalOpenApiExample/Program.cs
        productsV1.MapCreateProductsEndpoint();
        productsV1.MapUpdateProductEndpoint();
        productsV1.MapDebitProductStockEndpoint();
        productsV1.MapReplenishProductStockEndpoint();
        productsV1.MapGetProductByIdEndpoint();
        productsV1.MapGetProductsByPageEndpoint();
        productsV1.MapGetProductsViewEndpoint();

        return endpoints;
    }
}
