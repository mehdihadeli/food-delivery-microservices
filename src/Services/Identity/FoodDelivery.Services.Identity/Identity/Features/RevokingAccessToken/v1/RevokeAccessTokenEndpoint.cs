using AutoMapper;
using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Minimal.Extensions;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Identity.Identity.Features.RevokingAccessToken.V1;

public static class RevokeAccessTokenEndpoint
{
    public static RouteHandlerBuilder MapRevokeAccessTokenEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapPost("/revoke-token", Handle)
            .RequireAuthorization(IdentityConstants.Role.User)
            .MapToApiVersion(1.0)
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?#typedresults-vs-results
            // .Produces(StatusCodes.Status204NoContent)
            // .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .WithName(nameof(RevokeAccessToken))
            .WithDisplayName(nameof(RevokeAccessToken).Humanize())
            .WithSummaryAndDescription(nameof(RevokeAccessToken).Humanize(), nameof(RevokeAccessToken).Humanize());

        async Task<Results<NoContent, ValidationProblem>> Handle(
            [AsParameters] RevokeAccessTokenRequestParameters requestParameters
        )
        {
            var (request, context, commandProcessor, mapper, cancellationToken) = requestParameters;
            var token = string.IsNullOrWhiteSpace(request.AccessToken)
                ? GetTokenFromHeader(context)
                : request.AccessToken;

            var command = RevokeAccessToken.Of(token, context.User.Identity!.Name!);
            await commandProcessor.SendAsync(command, cancellationToken);

            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-7.0#multiple-response-types
            return TypedResults.NoContent();
        }
    }

    private static string GetTokenFromHeader(HttpContext context)
    {
        var authorizationHeader = context.Request.Headers.Get<string>("authorization");
        return authorizationHeader;
    }
}

public record RevokeAccessTokenRequest(string? AccessToken);

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#parameter-binding-for-argument-lists-with-asparameters
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#binding-precedence
internal record RevokeAccessTokenRequestParameters(
    [FromBody] RevokeAccessTokenRequest Request,
    HttpContext HttpContext,
    ICommandBus CommandProcessor,
    IMapper Mapper,
    CancellationToken CancellationToken
) : IHttpCommand<RevokeAccessTokenRequest>;
