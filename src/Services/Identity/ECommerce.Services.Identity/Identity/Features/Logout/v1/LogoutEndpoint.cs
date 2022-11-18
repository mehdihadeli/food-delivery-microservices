using BuildingBlocks.Abstractions.Caching;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Security.Jwt;
using EasyCaching.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace ECommerce.Services.Identity.Identity.Features.Logout.v1;

public static class LogoutEndpoint
{
    internal static RouteHandlerBuilder MapLogoutEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/logout", Logout)
            .Produces(StatusCodes.Status200OK)
            .RequireAuthorization()
            .WithName("logout")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Logout User", Description = "Logout User"
            })
            .WithDisplayName("Logout User.");
    }

    private static async Task<IResult> Logout(
        HttpContext httpContext,
        IEasyCachingProviderFactory cachingProviderFactory,
        IOptions<JwtOptions> jwtOptions)
    {
        var cacheProvider = cachingProviderFactory.GetCachingProvider(nameof(CacheProviderType.InMemory));

        await httpContext.SignOutAsync();

        if (jwtOptions.Value.CheckRevokedAccessTokens)
        {
            // https://dev.to/chukwutosin_/how-to-invalidate-a-jwt-using-a-blacklist-28dl
            // https://supertokens.com/blog/revoking-access-with-a-jwt-blacklist
            // The blacklist is saved in the format => "userName_revoked_tokens": [token1, token2,...]
            var token = GetTokenFromHeader(httpContext);
            var userName = httpContext.User.Identity!.Name;
            await cacheProvider.SetAsync(
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
