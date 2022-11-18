using BuildingBlocks.Abstractions.CQRS.Commands;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Services.Catalogs.Products.Features.DebitingProductStock.v1;

// POST api/v1/catalog/products/{productId}/debit-stock
public static class DebitProductStockEndpoint
{
    internal static RouteHandlerBuilder MapDebitProductStockEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/{productId}/debit-stock", DebitProductStock)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithMetadata(new SwaggerOperationAttribute("Debiting Product Stock", "Debiting Product Stock"))
            .WithName("DebitProductStock")
            .WithDisplayName("Debit product stock");
    }

    private static async Task<IResult> DebitProductStock(
        long productId,
        int quantity,
        ICommandProcessor commandProcessor,
        CancellationToken cancellationToken)
    {
        using (Serilog.Context.LogContext.PushProperty("Endpoint", nameof(DebitProductStockEndpoint)))
        using (Serilog.Context.LogContext.PushProperty("ProductId", productId))
        {
            await commandProcessor.SendAsync(new DebitProductStock(productId, quantity), cancellationToken);

            return Results.NoContent();
        }
    }
}
