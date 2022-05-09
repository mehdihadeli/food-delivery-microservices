using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Store.Services.Identity.Identity.Features.Logout;

public static class LogoutEndpoint
{
    internal static IEndpointRouteBuilder MapLogoutEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"{IdentityConfigs.IdentityPrefixUri}/logout", async (HttpContext httpContext) =>
            {
                await httpContext.SignOutAsync();
                return Results.Ok();
            })
            .Produces(StatusCodes.Status200OK)
            .WithTags(IdentityConfigs.Tag)
            .RequireAuthorization()
            .WithDisplayName("Logout User.");

        return endpoints;
    }
}
