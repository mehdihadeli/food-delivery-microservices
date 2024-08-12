using AutoMapper;
using BuildingBlocks.Abstractions.Core.Paging;
using BuildingBlocks.Abstractions.Queries;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Core.Paging;
using BuildingBlocks.Web.Minimal.Extensions;
using BuildingBlocks.Web.Problem.HttpResults;
using FoodDelivery.Services.Catalogs.Products.Dtos.v1;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Catalogs.Products.Features.GettingProductsView.V1;

// GET api/v1/catalog/products
public static class GetProductsViewEndpoint
{
    internal static RouteHandlerBuilder MapGetProductsViewEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapGet("/products-view/{page}/{pageSize}", Handle)
            .WithTags(ProductsConfigs.Tag)
            // .RequireAuthorization()
            .WithDisplayName(nameof(v1.GetProductsView).Humanize())
            .WithSummaryAndDescription(nameof(v1.GetProductsView).Humanize(), nameof(v1.GetProductsView).Humanize())
            .WithName(nameof(v1.GetProductsView))
            // .Produces<GetProductsViewResult>(StatusCodes.Status200OK)
            // .ProducesProblem(StatusCodes.Status401Unauthorized)
            // .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .MapToApiVersion(1.0);

        async Task<Results<Ok<GetProductsViewResponse>, ValidationProblem, UnAuthorizedHttpProblemResult>> Handle(
            [AsParameters] GetProductsViewRequestParameters requestParameters
        )
        {
            var (context, queryProcessor, mapper, cancellationToken, _, _, _, _) = requestParameters;
            var query = GetProductsView.Of(
                new PageRequest
                {
                    PageNumber = requestParameters.PageNumber,
                    PageSize = requestParameters.PageSize,
                    Filters = requestParameters.Filters,
                    SortOrder = requestParameters.SortOrder
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
    IMapper Mapper,
    CancellationToken CancellationToken,
    int PageSize = 10,
    int PageNumber = 1,
    string? Filters = null,
    string? SortOrder = null
) : IHttpQuery, IPageRequest;

internal record GetProductsViewResponse(IList<ProductViewDto> Products);
