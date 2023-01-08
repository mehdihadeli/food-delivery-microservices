using BuildingBlocks.Abstractions.CQRS.Commands;
using Hellang.Middleware.ProblemDetails;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Services.Identity.Identity.Features.RevokingRefreshToken.v1;

public static class RevokeRefreshTokenEndpoint
{
    internal static IEndpointRouteBuilder MapRevokeTokenEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/revoke-refresh-token", RevokeToken)
            .RequireAuthorization()
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status204NoContent)
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status400BadRequest)
            .WithDisplayName("Revoke refresh token.")
            .WithMetadata(new SwaggerOperationAttribute("Revoking Refresh Token", "Revoking Refresh Token"));

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
