using BuildingBlocks.Abstractions.Core.Paging;
using BuildingBlocks.Abstractions.Queries;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Core.Paging;
using BuildingBlocks.Web.ProblemDetail.HttpResults;
using FoodDelivery.Services.Catalogs.Products.Dtos.v1;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Catalogs.Products.Features.GettingProductsView.v1;

// GET api/v1/catalog/products
public static class GetProductsViewEndpoint
{
    internal static RouteHandlerBuilder MapGetProductsViewEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapGet("/products-view/{page}/{pageSize}", Handle)
            .WithTags(ProductsConfigurations.Tag)
            // .RequireAuthorization()
            .WithDisplayName(nameof(GetProductsView).Humanize())
            .WithSummary(nameof(GetProductsView).Humanize())
            .WithDescription(nameof(GetProductsView).Humanize())
            .WithName(nameof(GetProductsView))
            // .Produces<GetProductsViewResult>(StatusCodes.Status200OK)
            // .ProducesProblem(StatusCodes.Status401Unauthorized)
            // .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .MapToApiVersion(1.0);

        async Task<Results<Ok<GetProductsViewResponse>, ValidationProblem, UnAuthorizedHttpProblemResult>> Handle(
            [AsParameters] GetProductsViewRequestParameters requestParameters
        )
        {
            var (context, queryProcessor, cancellationToken, _, _, _, _) = requestParameters;
            var query = GetProductsView.Of(
                new PageRequest
                {
                    PageNumber = requestParameters.PageNumber,
                    PageSize = requestParameters.PageSize,
                    Filters = requestParameters.Filters,
                    SortOrder = requestParameters.SortOrder,
                }
            );

            var result = await queryProcessor.SendAsync(query, cancellationToken);

            return TypedResults.Ok(new GetProductsViewResponse(result.Products.ToList()));
        }
    }
}

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#parameter-binding-for-argument-lists-with-asparameters
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#binding-precedence
internal record GetProductsViewRequestParameters(
    HttpContext HttpContext,
    IQueryBus QueryBus,
    CancellationToken CancellationToken,
    int PageSize = 10,
    int PageNumber = 1,
    string? Filters = null,
    string? SortOrder = null
) : IHttpQuery, IPageRequest;

internal record GetProductsViewResponse(IList<ProductViewDto> Products);
