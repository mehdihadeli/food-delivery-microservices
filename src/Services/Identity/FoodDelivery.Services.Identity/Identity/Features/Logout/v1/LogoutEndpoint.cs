using AutoMapper;
using BuildingBlocks.Abstractions.Caching;
using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Security.Jwt;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Minimal.Extensions;
using EasyCaching.Core;
using Humanizer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;

namespace FoodDelivery.Services.Identity.Identity.Features.Logout.V1;

public static class LogoutEndpoint
{
    internal static RouteHandlerBuilder MapLogoutEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapPost("/logout", Handle)
            .RequireAuthorization()
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?#typedresults-vs-results
            // .Produces(StatusCodes.Status200OK)
            .WithName("Logout")
            .WithDisplayName("Logout".Humanize())
            .WithSummaryAndDescription("Logout".Humanize(), "Logout".Humanize())
            .MapToApiVersion(1.0);

        async Task<Ok> Handle([AsParameters] LogoutRequestParameters requestParameters)
        {
            var (context, commandProcessor, mapper, caching, jwtOptions, cancellationToken) = requestParameters;
            var cacheProvider = caching.GetCachingProvider(nameof(CacheProviderType.InMemory));

            await context.SignOutAsync();

            if (jwtOptions.Value.CheckRevokedAccessTokens)
            {
                // https://dev.to/chukwutosin_/how-to-invalidate-a-jwt-using-a-blacklist-28dl
                // https://supertokens.com/blog/revoking-access-with-a-jwt-blacklist
                // The blacklist is saved in the format => "userName_revoked_tokens": [token1, token2,...]
                var token = GetTokenFromHeader(context);
                var userName = context.User.Identity!.Name;
                await cacheProvider.SetAsync(
                    $"{userName}_{token}_revoked_token",
                    token,
                    TimeSpan.FromSeconds(jwtOptions.Value.TokenLifeTimeSecond)
                );
            }

            return TypedResults.Ok();
        }
    }

    private static string GetTokenFromHeader(HttpContext context)
    {
        var authorizationHeader = context.Request.Headers.Get<string>("authorization");
        return authorizationHeader;
    }
}

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#parameter-binding-for-argument-lists-with-asparameters
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#binding-precedence
internal record LogoutRequestParameters(
    [FromBody] HttpContext HttpContext,
    ICommandBus CommandProcessor,
    IMapper Mapper,
    IEasyCachingProviderFactory CachingProviderFactory,
    IOptions<JwtOptions> JwtOptions,
    CancellationToken CancellationToken
) : IHttpCommand;
