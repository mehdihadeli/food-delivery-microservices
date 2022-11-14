using BuildingBlocks.Abstractions.CQRS.Commands;
using ECommerce.Services.Identity.Shared;
using Asp.Versioning.Conventions;

namespace ECommerce.Services.Identity.Identity.Features.RefreshingToken;

public static class RefreshTokenEndpoint
{
    internal static IEndpointRouteBuilder MapRefreshTokenEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"{IdentityConfigs.IdentityPrefixUri}/refresh-token", RefreshToken)
            .WithTags(IdentityConfigs.Tag)
            .RequireAuthorization()
            .Produces<RefreshTokenResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .WithDisplayName("Refresh Token.")
            .WithApiVersionSet(IdentityConfigs.VersionSet)
            .HasApiVersion(1.0);

        return endpoints;
    }

    private static async Task<IResult> RefreshToken(
        RefreshTokenRequest request,
        ICommandProcessor commandProcessor,
        CancellationToken cancellationToken)
    {
        var command = new RefreshToken(request.AccessToken, request.RefreshToken);

        var result = await commandProcessor.SendAsync(command, cancellationToken);

        return Results.Ok(result);
    }
}
