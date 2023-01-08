using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Core.Extensions;
using ECommerce.Services.Identity.Identity.Features.RevokingAccessToken.v1;
using Hellang.Middleware.ProblemDetails;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Services.Identity.Identity.Features.RevokingAllAccessTokens.v1;

public static class RevokeAllAccessTokensEndpoint
{
    public static IEndpointRouteBuilder MapRevokeAllAccessTokensEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/revoke-all-tokens", RevokeAllAccessTokens)
            .RequireAuthorization(IdentityConstants.Role.User)
            .Produces<bool>()
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status200OK)
            .WithDisplayName("Revoke All Access Tokens.")
            .WithMetadata(new SwaggerOperationAttribute("Revoking All Tokens", "Revoking All Tokens"));

        return endpoints;
    }

    private static async Task<IResult> RevokeAllAccessTokens(
        HttpContext httpContext,
        RevokeAccessTokenRequest? request,
        ICommandProcessor commandProcessor,
        CancellationToken cancellationToken)
    {
        var command = new RevokeAllAccessTokens(httpContext.User.Identity!.Name!);

        await commandProcessor.SendAsync(command, cancellationToken);

        return Results.Ok();
    }

    private static string GetTokenFromHeader(HttpContext context)
    {
        var authorizationHeader = context.Request.Headers.Get<string>("authorization");
        return authorizationHeader;
    }
}
