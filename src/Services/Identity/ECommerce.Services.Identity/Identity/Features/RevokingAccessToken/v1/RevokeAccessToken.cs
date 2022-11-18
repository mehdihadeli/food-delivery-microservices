using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Caching;
using BuildingBlocks.Core.Exception.Types;
using BuildingBlocks.Security.Jwt;
using EasyCaching.Core;
using Microsoft.Extensions.Options;

namespace ECommerce.Services.Identity.Identity.Features.RevokingAccessToken.v1;

public record RevokeAccessToken(string Token, string UserName) : ICommand;

public class RevokeAccessTokenHandler : ICommandHandler<RevokeAccessToken>
{
    private readonly IEasyCachingProvider _cachingProvider;
    private readonly JwtOptions _jwtOptions;

    public RevokeAccessTokenHandler(
        IEasyCachingProviderFactory cachingProviderFactory,
        IOptions<JwtOptions> jwtOptions,
        IOptions<CacheOptions> cacheOptions)
    {
        Guard.Against.Null(cacheOptions);
        _cachingProvider = Guard.Against.Null(cachingProviderFactory).GetCachingProvider(cacheOptions.Value.DefaultCacheType);
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<Unit> Handle(RevokeAccessToken request, CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Token))
            throw new BadRequestException("Token is empty.");

        // https://dev.to/chukwutosin_/how-to-invalidate-a-jwt-using-a-blacklist-28dl
        // https://supertokens.com/blog/revoking-access-with-a-jwt-blacklist
        // The blacklist is saved in the format => "userName_revoked_tokens": [token1, token2,...]
        await _cachingProvider.SetAsync(
            $"{request.UserName}_{request.Token}_revoked_token",
            request.Token,
            TimeSpan.FromSeconds(_jwtOptions.TokenLifeTimeSecond),
            cancellationToken);

        return Unit.Value;
    }
}
