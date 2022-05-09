using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Query;

namespace Store.Services.Catalogs.Products.Features.GettingProductById;

// GET api/v1/catalog/products/{id}
public static class GetProductByIdEndpoint
{
    internal static IEndpointRouteBuilder MapGetProductByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
                $"{ProductsConfigs.ProductsPrefixUri}/{{id}}",
                GetProductById)
            .WithTags(ProductsConfigs.Tag)
            // .RequireAuthorization()
            .Produces<GetProductByIdResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("GetProductById")
            .WithDisplayName("Get product By Id.");

        return endpoints;
    }

    private static async Task<IResult> GetProductById(
        long id,
        IQueryProcessor queryProcessor,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(id, nameof(id));

        var result = await queryProcessor.SendAsync(new GetProductById(id), cancellationToken);

        return Results.Ok(result);
    }
}
