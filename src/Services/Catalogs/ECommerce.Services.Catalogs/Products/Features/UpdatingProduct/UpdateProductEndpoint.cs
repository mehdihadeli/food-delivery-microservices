using Ardalis.GuardClauses;
using Asp.Versioning.Conventions;
using BuildingBlocks.Abstractions.CQRS.Commands;
using ECommerce.Services.Catalogs.Shared;

namespace ECommerce.Services.Catalogs.Products.Features.UpdatingProduct;

// PUT api/v1/catalog/products/{id}
public static class UpdateProductEndpoint
{
    internal static IEndpointRouteBuilder MapUpdateProductEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                $"{ProductsConfigs.ProductsPrefixUri}/{{id}}",
                UpdateProducts)
            .WithTags(ProductsConfigs.Tag)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("UpdateProduct")
            .WithDisplayName("Update a product.")
            .WithApiVersionSet(SharedModulesConfiguration.VersionSet)
            .HasApiVersion(1.0);

        return endpoints;
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
