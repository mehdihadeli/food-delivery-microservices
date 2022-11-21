using BuildingBlocks.Abstractions.CQRS.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Services.Catalogs.Products.Features.GettingProductsView.v1;

// GET api/v1/catalog/products
public static class GetProductsViewEndpoint
{
    internal static RouteHandlerBuilder MapGetProductsViewEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/products-view/{page}/{pageSize}", GetProductsView)
            // .RequireAuthorization()
            .Produces<GetProductsViewResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest)
            .WithDisplayName("Get Products View.")
            .WithName("GetProductsView")
            .WithMetadata(new SwaggerOperationAttribute("Getting Products View", "Getting Products View"));
    }

    private static async Task<IResult> GetProductsView(
        IQueryProcessor queryProcessor,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 20)
    {
        using (Serilog.Context.LogContext.PushProperty("Endpoint", nameof(GetProductsViewEndpoint)))
        {
            var result = await queryProcessor.SendAsync(
                new GetProductsView {Page = page, PageSize = pageSize},
                cancellationToken);

            return Results.Ok(result);
        }
    }
}
