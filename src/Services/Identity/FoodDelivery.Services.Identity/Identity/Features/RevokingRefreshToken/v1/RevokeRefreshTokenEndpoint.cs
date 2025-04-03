using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Web.ProblemDetail.HttpResults;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Identity.Identity.Features.RevokingRefreshToken.v1;

public static class RevokeRefreshTokenEndpoint
{
    internal static IEndpointRouteBuilder MapRevokeTokenEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapPost("/revoke-refresh-token", Handle)
            .RequireAuthorization()
            .MapToApiVersion(1.0)
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?#typedresults-vs-results
            // .Produces(StatusCodes.Status204NoContent)
            // .ProducesProblem(StatusCodes.Status404NotFound)
            // .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .WithName(nameof(RevokeRefreshToken))
            .WithDisplayName(nameof(RevokeRefreshToken).Humanize())
            .WithSummary(nameof(RevokeRefreshToken).Humanize())
            .WithDescription(nameof(RevokeRefreshToken).Humanize());

        return endpoints;

        async Task<Results<NoContent, NotFoundHttpProblemResult, ValidationProblem>> Handle(
            [AsParameters] RevokeRefreshTokenRequestParameters requestParameters
        )
        {
            var (request, context, commandBus, cancellationToken) = requestParameters;

            var command = RevokeRefreshToken.Of(request.RefreshToken);
            await commandBus.SendAsync(command, cancellationToken);

            return TypedResults.NoContent();
        }
    }
}

public record RevokeRefreshTokenRequest(string? RefreshToken);

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#parameter-binding-for-argument-lists-with-asparameters
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#binding-precedence
internal record RevokeRefreshTokenRequestParameters(
    [FromBody] RevokeRefreshTokenRequest Request,
    HttpContext HttpContext,
    ICommandBus CommandBus,
    CancellationToken CancellationToken
) : IHttpCommand<RevokeRefreshTokenRequest>;
