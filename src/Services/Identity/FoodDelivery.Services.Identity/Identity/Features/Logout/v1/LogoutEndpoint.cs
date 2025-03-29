using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Core.Web.Extensions;
using BuildingBlocks.Security.Jwt;
using Humanizer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;

namespace FoodDelivery.Services.Identity.Identity.Features.Logout.v1;

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
            .WithSummary("Logout".Humanize())
            .WithDescription("Logout".Humanize())
            .MapToApiVersion(1.0);

        async Task<Ok> Handle([AsParameters] LogoutRequestParameters requestParameters)
        {
            var (context, commandBus, hybridCache, jwtOptions, cancellationToken) = requestParameters;

            await context.SignOutAsync();

            if (jwtOptions.Value.CheckRevokedAccessTokens)
            {
                // https://dev.to/chukwutosin_/how-to-invalidate-a-jwt-using-a-blacklist-28dl
                // https://supertokens.com/blog/revoking-access-with-a-jwt-blacklist
                // The blacklist is saved in the format => "userName_revoked_tokens": [token1, token2,...]
                var token = GetTokenFromHeader(context);
                var userName = context.User.Identity!.Name;

                // https://learn.microsoft.com/en-us/aspnet/core/performance/caching/hybrid#the-setasync-method
                await hybridCache.SetAsync(
                    key: $"{userName}_{token}_revoked_token",
                    value: GetTokenFromHeader(context),
                    options: null,
                    tags: null,
                    cancellationToken
                );
            }

            return TypedResults.Ok();
        }
    }

    private static string? GetTokenFromHeader(HttpContext context)
    {
        var authorizationHeader = context.Request.Headers.Get<string>("authorization");

        return authorizationHeader;
    }
}

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#parameter-binding-for-argument-lists-with-asparameters
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#binding-precedence
internal record LogoutRequestParameters(
    [FromBody] HttpContext HttpContext,
    ICommandBus CommandBus,
    HybridCache HybridCache,
    IOptions<JwtOptions> JwtOptions,
    CancellationToken CancellationToken
) : IHttpCommand;
