using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Commands;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Services.Catalogs.Products.Features.UpdatingProduct.v1;

// PUT api/v1/catalog/products/{id}
public static class UpdateProductEndpoint
{
    internal static RouteHandlerBuilder MapUpdateProductEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/{id}", UpdateProducts)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest)
            .HasApiVersion(2.0)
            .WithMetadata(new SwaggerOperationAttribute("Updating Product", "Updating Product"))
            .WithName("UpdateProduct")
            .WithDisplayName("Update a product.");
    }

    private static async Task<IResult> UpdateProducts(
        long id,
        UpdateProductRequest request,
        ICommandProcessor commandProcessor,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(request, nameof(request));
        var command = new UpdateProduct(
            id,
            request.Name,
            request.Price,
            request.RestockThreshold,
            request.MaxStockThreshold,
            request.Status,
            request.Width,
            request.Height,
            request.Depth,
            request.Size,
            request.CategoryId,
            request.SupplierId,
            request.BrandId,
            request.Description);

        using (Serilog.Context.LogContext.PushProperty("Endpoint", nameof(UpdateProductEndpoint)))
        using (Serilog.Context.LogContext.PushProperty("ProductId", command.Id))
        {
            await commandProcessor.SendAsync(command, cancellationToken);

            return Results.NoContent();
        }
    }
}
