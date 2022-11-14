using Asp.Versioning.Conventions;
using BuildingBlocks.Abstractions.CQRS.Queries;
using ECommerce.Services.Catalogs.Shared;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Services.Catalogs.Products.Features.GettingProductsView;

// GET api/v1/catalog/products
public static class GetProductsViewEndpoint
{
    internal static IEndpointRouteBuilder MapGetProductsViewEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
                $"{SharedModulesConfiguration.CatalogModulePrefixUri}/products-view/{{page}}/{{pageSize}}",
                GetProductsView)
            // .RequireAuthorization()
            .Produces<GetProductsViewResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest)
            .WithDisplayName("Get Products View.")
            .WithName("GetProductsView")
            .WithTags(ProductsConfigs.Tag)
            .WithMetadata(new SwaggerOperationAttribute("Getting Products View", "Getting Products View"))
            .WithApiVersionSet(ProductsConfigs.VersionSet)
            // .IsApiVersionNeutral()
            // .MapToApiVersion(1.0)
            .HasApiVersion(1.0);

        return endpoints;
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
