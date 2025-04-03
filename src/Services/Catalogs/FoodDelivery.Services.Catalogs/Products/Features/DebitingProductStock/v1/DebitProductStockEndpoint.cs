using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Web.ProblemDetail.HttpResults;
using Cassandra.Mapping;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Catalogs.Products.Features.DebitingProductStock.v1;

// POST api/v1/catalog/products/{productId}/debit-stock
public static class DebitProductStockEndpoint
{
    internal static RouteHandlerBuilder MapDebitProductStockEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapPost("/{productId}/debit-stock", Handle)
            .RequireAuthorization()
            .WithTags(ProductsConfigurations.Tag)
            .WithName(nameof(DebitProductStock))
            .WithDisplayName(nameof(DebitProductStock).Humanize())
            .WithSummary(nameof(DebitProductStock).Humanize())
            .WithDescription(nameof(DebitProductStock).Humanize())
            // .Produces("Debit-Stock performed successfully. (No Content)", StatusCodes.Status204NoContent)
            // .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            // .ProducesProblem(StatusCodes.Status401Unauthorized)
            // .ProducesProblem(StatusCodes.Status404NotFound)
            .MapToApiVersion(1.0);

        async Task<
            Results<NoContent, UnAuthorizedHttpProblemResult, ValidationProblem, NotFoundHttpProblemResult>
        > Handle([AsParameters] DebitProductStockRequestParameters requestParameters)
        {
            var (request, productId, context, commandBus, cancellationToken) = requestParameters;

            await commandBus.SendAsync(DebitProductStock.Of(productId, request.DebitQuantity), cancellationToken);

            return TypedResults.NoContent();
        }
    }
}

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#parameter-binding-for-argument-lists-with-asparameters
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#binding-precedence
internal record DebitProductStockRequestParameters(
    [FromBody] DebitProductStockRequest Request,
    [FromRoute] long ProductId,
    HttpContext HttpContext,
    ICommandBus CommandBus,
    CancellationToken CancellationToken
) : IHttpCommand<DebitProductStockRequest>;

internal record DebitProductStockRequest(int DebitQuantity);
