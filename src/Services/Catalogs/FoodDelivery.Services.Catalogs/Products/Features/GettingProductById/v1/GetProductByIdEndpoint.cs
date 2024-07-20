using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Web.Minimal.Extensions;
using BuildingBlocks.Web.Problem.HttpResults;
using FoodDelivery.Services.Catalogs.Products.Dtos.v1;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Catalogs.Products.Features.GettingProductById.v1;

// GET api/v1/catalog/products/{id}
internal static class GetProductByIdEndpoint
{
    internal static RouteHandlerBuilder MapGetProductByIdEndpoint(this IEndpointRouteBuilder app)
    {
        // return app.MapQueryEndpoint<GetProductByIdRequestParameters, GetProductByIdResponse, GetProductById,
        //         GetProductByIdResult>("/{id}")
        return app.MapGet("/{id}", Handle)
            // .RequireAuthorization()
            .WithTags(ProductsConfigs.Tag)
            .WithName(nameof(GetProductById))
            .WithDisplayName(nameof(GetProductById).Humanize())
            .WithSummaryAndDescription(nameof(GetProductById).Humanize(), nameof(GetProductById).Humanize())
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?#typedresults-vs-results
            // .Produces<GetProductByIdResponse>("Product fetched successfully.", StatusCodes.Status200OK)
            // .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            // .ProducesProblem(StatusCodes.Status404NotFound)
            // .ProducesProblem(StatusCodes.Status401Unauthorized)
            .MapToApiVersion(1.0);

        async Task<
            Results<
                Ok<GetProductByIdResponse>,
                ValidationProblem,
                NotFoundHttpProblemResult,
                UnAuthorizedHttpProblemResult
            >
        > Handle([AsParameters] GetProductByIdRequestParameters requestParameters)
        {
            var (id, _, queryProcessor, mapper, cancellationToken) = requestParameters;
            var result = await queryProcessor.SendAsync(GetProductById.Of(id), cancellationToken);

            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-7.0#multiple-response-types
            return TypedResults.Ok(new GetProductByIdResponse(result.Product));
        }
    }
}

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#parameter-binding-for-argument-lists-with-asparameters
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#binding-precedence
internal record GetProductByIdRequestParameters(
    [FromRoute] long Id,
    HttpContext HttpContext,
    IQueryProcessor QueryProcessor,
    IMapper Mapper,
    CancellationToken CancellationToken
) : IHttpQuery;

internal record GetProductByIdResponse(ProductDto Product);
