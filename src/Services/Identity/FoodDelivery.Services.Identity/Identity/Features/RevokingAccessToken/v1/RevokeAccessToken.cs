using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Caching;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Security.Jwt;
using EasyCaching.Core;
using Microsoft.Extensions.Options;

namespace FoodDelivery.Services.Identity.Identity.Features.RevokingAccessToken.v1;

internal record RevokeAccessToken(string Token, string UserName) : ICommand
{
    /// <summary>
    /// RevokeAccessToken with in-line validation.
    /// </summary>
    /// <param name="token"></param>
    /// <param name="userName"></param>
    /// <returns></returns>
    public static RevokeAccessToken Of(string? token, string? userName)
    {
        token.NotBeNullOrWhiteSpace();
        userName.NotBeNullOrWhiteSpace();

        return new RevokeAccessToken(token, userName);
    }
}

internal class RevokeAccessTokenHandler : ICommandHandler<RevokeAccessToken>
{
    private readonly IEasyCachingProvider _cachingProvider;
    private readonly JwtOptions _jwtOptions;

    public RevokeAccessTokenHandler(
        IEasyCachingProviderFactory cachingProviderFactory,
        IOptions<JwtOptions> jwtOptions,
        IOptions<CacheOptions> cacheOptions
    )
    {
        _cachingProvider = cachingProviderFactory.GetCachingProvider(cacheOptions.Value.DefaultCacheType);
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<Unit> Handle(RevokeAccessToken command, CancellationToken cancellationToken)
    {
        command.NotBeNull();
        command.Token.NotBeNullOrWhiteSpace();

        // https://dev.to/chukwutosin_/how-to-invalidate-a-jwt-using-a-blacklist-28dl
        // https://supertokens.com/blog/revoking-access-with-a-jwt-blacklist
        // The blacklist is saved in the format => "userName_revoked_tokens": [token1, token2,...]
        await _cachingProvider.SetAsync(
            $"{command.UserName}_{command.Token}_revoked_token",
            command.Token,
            TimeSpan.FromSeconds(_jwtOptions.TokenLifeTimeSecond),
            cancellationToken
        );

        return Unit.Value;
    }
}
