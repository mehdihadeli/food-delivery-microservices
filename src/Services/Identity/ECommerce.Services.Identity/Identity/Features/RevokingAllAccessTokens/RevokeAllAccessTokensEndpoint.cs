using Asp.Versioning.Conventions;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Core.Extensions;
using ECommerce.Services.Identity.Identity.Features.RevokingAllAccessTokens;
using ECommerce.Services.Identity.Shared;

namespace ECommerce.Services.Identity.Identity.Features.RevokingAccessToken;

public static class RevokeAllAccessTokensEndpoint
{
    public static IEndpointRouteBuilder MapRevokeAllAccessTokensEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"{IdentityConfigs.IdentityPrefixUri}/revoke-all-tokens", RevokeAllAccessTokens)
            .WithTags(IdentityConfigs.Tag)
            .RequireAuthorization(IdentityConstants.Role.User)
            .Produces<bool>()
            .Produces(StatusCodes.Status400BadRequest)
            .WithDisplayName("Revoke Current User Access Token From the Header.")
            .WithApiVersionSet(IdentityConfigs.VersionSet)
            .HasApiVersion(1.0);

        return endpoints;
    }

    private static async Task<IResult> RevokeAllAccessTokens(
        HttpContext httpContext,
        RevokeAccessTokenRequest? request,
        ICommandProcessor commandProcessor,
        CancellationToken cancellationToken)
    {
        var command = new RevokeAllAccessTokens(httpContext.User.Identity!.Name!);

        var result = await commandProcessor.SendAsync(command, cancellationToken);

        return Results.Ok(result);
    }

    private static string GetTokenFromHeader(HttpContext context)
    {
        var authorizationHeader = context.Request.Headers.Get<string>("authorization");
        return authorizationHeader;
    }
}
