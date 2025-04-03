using BuildingBlocks.Caching;
using BuildingBlocks.Core.Exception.Types;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;

namespace FoodDelivery.Services.Identity.Api.Middlewares;

public class RevokeAccessTokenMiddleware(IOptions<CacheOptions> options, HybridCache hybridCache) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.User.Identity is null || string.IsNullOrWhiteSpace(context.User.Identity.Name))
        {
            await next(context);
            return;
        }

        var accessToken = GetTokenFromHeader(context);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            await next(context);
            return;
        }

        var userName = context.User.Identity.Name;
        var cacheKey = $"{options.Value.DefaultCachePrefix}{userName}_{accessToken}_revoked_token";

        // Use GetOrCreateAsync to check if the key exists without creating it
        var revokedToken = await hybridCache.GetOrCreateAsync<string>(
            cacheKey,
            _ => ValueTask.FromResult<string>(null!)
        );

        if (revokedToken is not null)
        {
            // If the token is found in the cache, it's revoked
            throw new UnAuthorizedException("Access token is revoked, User is not authorized to access this resource");
        }

        await next(context);
    }

    private static string? GetTokenFromHeader(HttpContext context)
    {
        var authorizationHeader = context.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrWhiteSpace(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
        {
            return null;
        }

        return authorizationHeader.Substring("Bearer ".Length).Trim();
    }
}

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseRevokeAccessTokenMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RevokeAccessTokenMiddleware>();
    }
}
