using BuildingBlocks.Core.Extensions;
using Mediator;
using Microsoft.Extensions.Caching.Hybrid;
using ICommand = BuildingBlocks.Abstractions.Commands.ICommand;

namespace FoodDelivery.Services.Identity.Identity.Features.RevokingAccessToken.v1;

public record RevokeAccessToken(string Token, string UserName) : ICommand
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

public class RevokeAccessTokenHandler(HybridCache hybridCache)
    : BuildingBlocks.Abstractions.Commands.ICommandHandler<RevokeAccessToken>
{
    public async ValueTask<Unit> Handle(RevokeAccessToken command, CancellationToken cancellationToken)
    {
        command.NotBeNull();
        command.Token.NotBeNullOrWhiteSpace();

        // https://dev.to/chukwutosin_/how-to-invalidate-a-jwt-using-a-blacklist-28dl
        // https://supertokens.com/blog/revoking-access-with-a-jwt-blacklist
        // The blacklist is saved in the format => "userName_revoked_tokens": [token1, token2,...]
        await hybridCache.SetAsync(
            $"{command.UserName}_{command.Token}_revoked_token",
            command.Token,
            cancellationToken: cancellationToken
        );

        return Unit.Value;
    }
}
