using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Abstractions.Web.Module;
using FoodDelivery.Services.Catalogs.Products.Data;
using FoodDelivery.Services.Catalogs.Products.Features.CreatingProduct.v1;
using FoodDelivery.Services.Catalogs.Products.Features.DebitingProductStock.v1;
using FoodDelivery.Services.Catalogs.Products.Features.GettingProductById.v1;
using FoodDelivery.Services.Catalogs.Products.Features.GettingProductsView.v1;
using FoodDelivery.Services.Catalogs.Products.Features.ReplenishingProductStock.v1;
using FoodDelivery.Services.Catalogs.Products.Features.UpdatingProduct.v1;
using FoodDelivery.Services.Catalogs.Shared;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FoodDelivery.Services.Catalogs.Products;

internal class ProductsConfigs : IModuleConfiguration
{
    public const string Tag = "Products";
    public const string ProductsPrefixUri = $"{SharedModulesConfiguration.CatalogModulePrefixUri}/products";

    public WebApplicationBuilder AddModuleServices(WebApplicationBuilder builder)
    {
        builder.Services.TryAddSingleton<IEventMapper, ProductEventMapper>();

        return builder;
    }

    public Task<WebApplication> ConfigureModule(WebApplication app)
    {
        return Task.FromResult(app);
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
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
        productsV1.MapGetProductsViewEndpoint();

        return endpoints;
    }
}
