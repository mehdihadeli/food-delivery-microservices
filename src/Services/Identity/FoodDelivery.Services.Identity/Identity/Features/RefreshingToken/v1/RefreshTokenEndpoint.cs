using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Web.ProblemDetail.HttpResults;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Identity.Identity.Features.RefreshingToken.v1;

public static class RefreshTokenEndpoint
{
    internal static RouteHandlerBuilder MapRefreshTokenEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapPost("/refresh-token", Handle)
            .RequireAuthorization()
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?#typedresults-vs-results
            // .Produces<RefreshTokenResult>(StatusCodes.Status200OK))
            // .ProducesProblem(StatusCodes.Status404NotFound)
            // .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .WithName(nameof(RefreshToken))
            .WithDisplayName(nameof(RefreshToken).Humanize())
            .WithSummary(nameof(RefreshToken).Humanize())
            .WithDescription(nameof(RefreshToken).Humanize());

        async Task<Results<Ok<RefreshTokenResponse>, NotFoundHttpProblemResult, ValidationProblem>> Handle(
            [AsParameters] RefreshTokenRequestParameters requestParameters
        )
        {
            var (request, context, commandBus, cancellationToken) = requestParameters;

            var command = RefreshToken.Of(request.AccessToken, request.RefreshToken);

            var result = await commandBus.SendAsync(command, cancellationToken);

            var response = result.ToRefreshTokenResponse();

            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-7.0#multiple-response-types
            return TypedResults.Ok(response);
        }
    }
}

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#parameter-binding-for-argument-lists-with-asparameters
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#binding-precedence
internal record RefreshTokenRequestParameters(
    [FromBody] RefreshTokenRequest Request,
    HttpContext HttpContext,
    ICommandBus CommandBus,
    CancellationToken CancellationToken
) : IHttpCommand<RefreshTokenRequest>;

// These parameters can be pass null from the user
public record RefreshTokenRequest(string? AccessToken, string? RefreshToken);

internal record RefreshTokenResponse(
    Guid UserId,
    string UserName,
    string FirstName,
    string LastName,
    string AccessToken,
    string RefreshToken
);
