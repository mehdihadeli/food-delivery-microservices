using BuildingBlocks.Abstractions.Caching;
using BuildingBlocks.Core.Exception.Types;
using BuildingBlocks.Core.Extensions;

namespace ECommerce.Services.Identity.Api.Middlewares;

public class RevokeAccessTokenMiddleware : IMiddleware
{
    private readonly ICacheManager _cacheManager;

    public RevokeAccessTokenMiddleware(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.User.Identity is null || string.IsNullOrWhiteSpace(context.User.Identity.Name))
        {
            await next(context);
            return;
        }

        var accessToken = GetTokenFromHeader(context);
        var userName = context.User.Identity.Name;

        var revokedToken = await _cacheManager.DefaultCacheProvider.GetAsync<string>
            ($"{userName}_{accessToken}_revoked_token");
        if (string.IsNullOrWhiteSpace(revokedToken))
        {
             await next(context);
             return;
        }

        throw new UnAuthorizedException("Access token is revoked, User in not authorized to access this resource");
    }

    private static string GetTokenFromHeader(HttpContext context)
    {
        var authorizationHeader = context.Request.Headers.Get<string>("authorization");
        return authorizationHeader;
    }
}

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseRevokeAccessTokenMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RevokeAccessTokenMiddleware>();
    }
}
