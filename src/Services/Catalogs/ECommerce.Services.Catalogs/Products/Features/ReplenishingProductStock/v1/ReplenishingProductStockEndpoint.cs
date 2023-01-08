using BuildingBlocks.Abstractions.CQRS.Commands;
using Hellang.Middleware.ProblemDetails;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Services.Catalogs.Products.Features.ReplenishingProductStock.v1;

// POST api/v1/catalog/products/{productId}/replenish-stock
public static class ReplenishingProductStockEndpoint
{
    internal static RouteHandlerBuilder MapReplenishProductStockEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/{productId}/replenish-stock", ReplenishProductStock)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status404NotFound)
            .WithMetadata(new SwaggerOperationAttribute(
                "Replenishing ProductStock Products ",
                "Replenishing ProductStock Products"))
            .WithName("ReplenishProductStock")
            .WithDisplayName("Replenish product stock");
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
            await commandProcessor.SendAsync(new ReplenishProductStock(productId, quantity), cancellationToken);

            return Results.NoContent();
        }
    }
}
