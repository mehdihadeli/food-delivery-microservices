using BuildingBlocks.Abstractions.CQRS.Query;

namespace ECommerce.Services.Catalogs.Products.Features.GettingProductsView;

// GET api/v1/catalog/products
public static class GetProductsViewEndpoint
{
    internal static IEndpointRouteBuilder MapGetProductsViewEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
                $"{CatalogModuleConfiguration.CatalogModulePrefixUri}/products-view/{{page}}/{{pageSize}}",
                GetProductsView)
            .WithTags(ProductsConfigs.Tag)
            // .RequireAuthorization()
            .Produces<GetProductsViewResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest)
            .WithDisplayName("Get products.");

        return endpoints;
    }

    private static async Task<IResult> GetProductsView(
        IQueryProcessor queryProcessor,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 20)
    {
        var result = await queryProcessor.SendAsync(
            new GetProductsView { Page = page, PageSize = pageSize },
            cancellationToken);

        return Results.Ok(result);
    }
}
