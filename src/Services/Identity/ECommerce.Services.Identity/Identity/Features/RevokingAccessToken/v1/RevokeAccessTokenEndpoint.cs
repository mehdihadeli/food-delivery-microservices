using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Core.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Services.Identity.Identity.Features.RevokingAccessToken.v1;

public static class RevokeAccessTokenEndpoint
{
    public static RouteHandlerBuilder MapRevokeAccessTokenEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/revoke-token", RevokeAccessToken)
            .RequireAuthorization(IdentityConstants.Role.User)
            .Produces(StatusCodes.Status400BadRequest)
            .WithDisplayName("Revoke Current User Access Token From the Header.")
            .WithName("RevokeAccessToken")
            .WithMetadata(new SwaggerOperationAttribute(
                "Revoking Token",
                "Revoking Current User Access Token From the Header."));
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
