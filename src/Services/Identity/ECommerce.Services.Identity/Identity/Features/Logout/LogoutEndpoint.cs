using BuildingBlocks.Abstractions.Caching;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Security.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace ECommerce.Services.Identity.Identity.Features.Logout;

public static class LogoutEndpoint
{
    internal static IEndpointRouteBuilder MapLogoutEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"{IdentityConfigs.IdentityPrefixUri}/logout", Logout)
            .Produces(StatusCodes.Status200OK)
            .WithTags(IdentityConfigs.Tag)
            .RequireAuthorization()
            .WithDisplayName("Logout User.");

        return endpoints;
    }

    private static async Task<IResult> Logout(
        HttpContext httpContext,
        ICacheManager cacheManager,
        IOptions<JwtOptions> jwtOptions)
    {
        await httpContext.SignOutAsync();

        if (jwtOptions.Value.CheckRevokedAccessTokens)
        {
            // https://dev.to/chukwutosin_/how-to-invalidate-a-jwt-using-a-blacklist-28dl
            // https://supertokens.com/blog/revoking-access-with-a-jwt-blacklist
            // The blacklist is saved in the format => "userName_revoked_tokens": [token1, token2,...]
            var token = GetTokenFromHeader(httpContext);
            var userName = httpContext.User.Identity!.Name;
            await cacheManager.DefaultCacheProvider.SetAsync(
                $"{userName}_{token}_revoked_token",
                token,
                TimeSpan.FromSeconds(jwtOptions.Value.TokenLifeTimeSecond));
        }

        return Results.Ok();
    }

    private static string GetTokenFromHeader(HttpContext context)
    {
        var authorizationHeader = context.Request.Headers.Get<string>("authorization");
        return authorizationHeader;
    }
}
