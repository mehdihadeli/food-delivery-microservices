using Ardalis.GuardClauses;
using Asp.Versioning.Conventions;
using BuildingBlocks.Abstractions.CQRS.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Services.Catalogs.Products.Features.GettingProductById;

// GET api/v1/catalog/products/{id}
public static class GetProductByIdEndpoint
{
    internal static IEndpointRouteBuilder MapGetProductByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
                $"{ProductsConfigs.ProductsPrefixUri}/{{id}}", GetProductById)
            // .RequireAuthorization()
            .Produces<GetProductByIdResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags(ProductsConfigs.Tag)
            .WithMetadata(new SwaggerOperationAttribute("Getting Product by Id", "Getting Product by Id"))
            .WithName("GetProductById")
            .WithDisplayName("Get product By Id.")
            .WithApiVersionSet(ProductsConfigs.VersionSet)
            .MapToApiVersion(1.0)
            .HasApiVersion(1.0);

        return endpoints;
    }

    private static async Task<IResult> GetProductById(
        long id,
        IQueryProcessor queryProcessor,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(id, nameof(id));

        using (Serilog.Context.LogContext.PushProperty("Endpoint", nameof(GetProductByIdEndpoint)))
        using (Serilog.Context.LogContext.PushProperty("ProductId", id))
        {
            var result = await queryProcessor.SendAsync(new GetProductById(id), cancellationToken);

            return Results.Ok(result);
        }
    }
}
