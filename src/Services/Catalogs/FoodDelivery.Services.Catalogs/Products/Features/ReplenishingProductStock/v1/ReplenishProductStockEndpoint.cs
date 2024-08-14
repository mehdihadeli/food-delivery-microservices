using AutoMapper;
using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Web.Minimal.Extensions;
using BuildingBlocks.Web.Problem.HttpResults;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Catalogs.Products.Features.ReplenishingProductStock.v1;

// POST api/v1/catalog/products/{productId}/replenish-stock
internal static class ReplenishProductStockEndpoint
{
    internal static RouteHandlerBuilder MapReplenishProductStockEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapPost("/{productId}/replenish-stock", Handle)
            .RequireAuthorization()
            .WithTags(ProductsConfigs.Tag)
            .WithName(nameof(ReplenishProductStock))
            .WithDisplayName(nameof(ReplenishProductStock).Humanize())
            .WithSummaryAndDescription(
                nameof(ReplenishProductStock).Humanize(),
                nameof(ReplenishProductStock).Humanize()
            )
            // .Produces("Debit-Stock performed successfully. (No Content)", StatusCodes.Status204NoContent)
            // .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            // .ProducesProblem(StatusCodes.Status401Unauthorized)
            // .ProducesProblem(StatusCodes.Status404NotFound)
            .MapToApiVersion(1.0);

        async Task<
            Results<NoContent, UnAuthorizedHttpProblemResult, ValidationProblem, NotFoundHttpProblemResult>
        > Handle([AsParameters] ReplenishProductStockRequestParameters requestParameters)
        {
            var (request, productId, context, commandBus, _, cancellationToken) = requestParameters;

            await commandBus.SendAsync(ReplenishProductStock.Of(productId, request.DebitQuantity), cancellationToken);

            return TypedResults.NoContent();
        }
    }
}

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#parameter-binding-for-argument-lists-with-asparameters
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#binding-precedence
internal record ReplenishProductStockRequestParameters(
    [FromBody] ReplenishProductStockRequest Request,
    [FromRoute] long ProductId,
    HttpContext HttpContext,
    ICommandBus CommandBus,
    IMapper Mapper,
    CancellationToken CancellationToken
) : IHttpCommand<ReplenishProductStockRequest>;

internal record ReplenishProductStockRequest(int DebitQuantity);
