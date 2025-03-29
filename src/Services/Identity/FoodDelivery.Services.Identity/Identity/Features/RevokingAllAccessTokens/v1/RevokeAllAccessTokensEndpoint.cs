using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Identity.Identity.Features.RevokingAllAccessTokens.v1;

public static class RevokeAllAccessTokensEndpoint
{
    public static IEndpointRouteBuilder MapRevokeAllAccessTokensEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapPost("/revoke-all-tokens", Handle)
            .RequireAuthorization(IdentityConstants.Role.User)
            .MapToApiVersion(1.0)
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?#typedresults-vs-results
            // .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            // .Produces(StatusCodes.Status204NoContent)
            .WithName(nameof(RevokeAllAccessTokens))
            .WithDisplayName(nameof(RevokeAllAccessTokens).Humanize())
            .WithSummary(nameof(RevokeAllAccessTokens).Humanize())
            .WithDescription(nameof(RevokeAllAccessTokens).Humanize());

        return endpoints;

        async Task<Results<NoContent, ValidationProblem>> Handle(
            [AsParameters] RevokeAllTokensRequestParameters requestParameters
        )
        {
            var (context, commandBus, cancellationToken) = requestParameters;

            var command = RevokeAllAccessTokens.Of(context.User.Identity!.Name!);

            await commandBus.SendAsync(command, cancellationToken);

            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-7.0#multiple-response-types
            return TypedResults.NoContent();
        }
    }
}

internal record RevokeAllTokensRequestParameters(
    HttpContext HttpContext,
    ICommandBus CommandBus,
    CancellationToken CancellationToken
) : IHttpCommand;
