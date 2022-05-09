using BuildingBlocks.Abstractions.CQRS.Command;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Store.Services.Identity.Identity.Features.RefreshingToken;

public static class RefreshTokenEndpoint
{
    internal static IEndpointRouteBuilder MapRefreshTokenEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"{IdentityConfigs.IdentityPrefixUri}/refresh-token", RefreshToken)
            .WithTags(IdentityConfigs.Tag)
            .RequireAuthorization()
            .Produces<RefreshTokenResult>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .WithDisplayName("Refresh Token.");

        return endpoints;
    }

    private static async Task<IResult> RefreshToken(
        RefreshTokenRequest request,
        ICommandProcessor commandProcessor,
        CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(request.AccessToken, request.RefreshToken);

        var result = await commandProcessor.SendAsync(command, cancellationToken);

        return Results.Ok(result);
    }
}
