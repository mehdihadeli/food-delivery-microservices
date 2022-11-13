using Asp.Versioning.Conventions;
using BuildingBlocks.Abstractions.CQRS.Commands;
using ECommerce.Services.Identity.Shared;

namespace ECommerce.Services.Identity.Identity.Features.RevokingRefreshToken;

public static class RevokeRefreshTokenEndpoint
{
    internal static IEndpointRouteBuilder MapRevokeTokenEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"{IdentityConfigs.IdentityPrefixUri}/revoke-refresh-token", RevokeToken)
            .WithTags(IdentityConfigs.Tag)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .WithDisplayName("Revoke refresh token.")
            .WithApiVersionSet(SharedModulesConfiguration.VersionSet)
            .HasApiVersion(1.0);

        return endpoints;
    }

    private static async Task<IResult> RevokeToken(
        RevokeRefreshTokenRequest request,
        ICommandProcessor commandProcessor,
        CancellationToken cancellationToken)
    {
        var command = new RevokeRefreshToken(request.RefreshToken);

        await commandProcessor.SendAsync(command, cancellationToken);

        return Results.NoContent();
    }
}
