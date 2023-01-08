using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Web.Extensions;
using Hellang.Middleware.ProblemDetails;
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
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status404NotFound)
            .WithResponseDescription(StatusCodes.Status401Unauthorized, "Unauthorized")
            .WithResponseDescription(StatusCodes.Status400BadRequest, "Invalid inputs. (Bad Request)")
            .WithResponseDescription(StatusCodes.Status404NotFound, "Product Not Found. (Not Found)")
            .WithResponseDescription(StatusCodes.Status204NoContent, "Debit-Stock performed successfully. (No Content)")
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
