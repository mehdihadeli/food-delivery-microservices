using BuildingBlocks.Abstractions.CQRS.Commands;

namespace ECommerce.Services.Catalogs.Products.Features.ReplenishingProductStock;

// POST api/v1/catalog/products/{productId}/replenish-stock
public static class ReplenishingProductStockEndpoint
{
    internal static IEndpointRouteBuilder MapReplenishProductStockEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                $"{ProductsConfigs.ProductsPrefixUri}/{{productId}}/replenish-stock",
                ReplenishProductStock)
            .RequireAuthorization()
            .WithTags(ProductsConfigs.Tag)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("ReplenishProductStock")
            .WithDisplayName("Replenish product stock");

        return endpoints;
    }

    private static async Task<IResult> ReplenishProductStock(
        long productId,
        int quantity,
        ICommandProcessor commandProcessor,
        CancellationToken cancellationToken)
    {
        using (Serilog.Context.LogContext.PushProperty("Endpoint", nameof(ReplenishingProductStockEndpoint)))
        using (Serilog.Context.LogContext.PushProperty("ProductId", productId))
        {
            await commandProcessor.SendAsync(new ReplenishingProductStock(productId, quantity), cancellationToken);

            return Results.NoContent();
        }
    }
}
