using BuildingBlocks.Abstractions.CQRS.Commands;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Services.Identity.Identity.Features.RefreshingToken.v1;

public static class RefreshTokenEndpoint
{
    internal static RouteHandlerBuilder MapRefreshTokenEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/refresh-token", RefreshToken)
            .RequireAuthorization()
            .Produces<RefreshTokenResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("RefreshToken")
            .WithDisplayName("Refresh Token.")
            .WithMetadata(new SwaggerOperationAttribute("Refreshing Token", "Refreshing Token"));
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
