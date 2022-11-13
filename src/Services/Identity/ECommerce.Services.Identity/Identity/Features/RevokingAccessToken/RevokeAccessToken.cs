using BuildingBlocks.Abstractions.Caching;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Core.Exception.Types;
using BuildingBlocks.Security.Jwt;
using Microsoft.Extensions.Options;

namespace ECommerce.Services.Identity.Identity.Features.RevokingAccessToken;

public record RevokeAccessToken(string Token, string UserName) : ICommand;

public class RevokeAccessTokenHandler : ICommandHandler<RevokeAccessToken>
{
    private readonly ICacheManager _cacheManager;
    private readonly JwtOptions _jwtOptions;

    public RevokeAccessTokenHandler(ICacheManager cacheManager, IOptions<JwtOptions> jwtOptions)
    {
        _cacheManager = cacheManager;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<Unit> Handle(RevokeAccessToken request, CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Token))
            throw new BadRequestException("Token is empty.");

        // https://dev.to/chukwutosin_/how-to-invalidate-a-jwt-using-a-blacklist-28dl
        // https://supertokens.com/blog/revoking-access-with-a-jwt-blacklist
        // The blacklist is saved in the format => "userName_revoked_tokens": [token1, token2,...]
        await _cacheManager.SetAsync(
            $"{request.UserName}_{request.Token}_revoked_token",
            request.Token,
            Convert.ToInt32(_jwtOptions.TokenLifeTimeSecond));

        return Unit.Value;
    }
}
