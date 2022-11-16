using Asp.Versioning.Conventions;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Core.Extensions;
using ECommerce.Services.Identity.Shared;

namespace ECommerce.Services.Identity.Identity.Features.RevokingAccessToken;

public static class RevokeAccessTokenEndpoint
{
    public static IEndpointRouteBuilder MapRevokeAccessTokenEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"{IdentityConfigs.IdentityPrefixUri}/revoke-token", RevokeAccessToken)
            .WithTags(IdentityConfigs.Tag)
            .RequireAuthorization(IdentityConstants.Role.User)
            .Produces(StatusCodes.Status400BadRequest)
            .WithDisplayName("Revoke Current User Access Token From the Header.")
            .WithName("RevokeAccessToken")
            .WithApiVersionSet(IdentityConfigs.VersionSet)
            .HasApiVersion(1.0);

        return endpoints;
    }

    private static async Task<IResult> RevokeAccessToken(
        HttpContext httpContext,
        RevokeAccessTokenRequest? request,
        ICommandProcessor commandProcessor,
        CancellationToken cancellationToken)
    {
        string token;
        if (request is null || string.IsNullOrWhiteSpace(request.AccessToken))
        {
            token = GetTokenFromHeader(httpContext);
        }
        else
        {
            token = request.AccessToken;
        }

        var command = new RevokeAccessToken(token, httpContext.User.Identity!.Name!);

        var result = await commandProcessor.SendAsync(command, cancellationToken);

        return Results.Ok(result);
    }

    private static string GetTokenFromHeader(HttpContext context)
    {
        var authorizationHeader = context.Request.Headers.Get<string>("authorization");
        return authorizationHeader;
    }
}
