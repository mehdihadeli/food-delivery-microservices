using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Identity.Identity.Exceptions;
using FoodDelivery.Services.Identity.Identity.Features.RefreshingToken.v1;
using FoodDelivery.Services.Identity.Shared.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;
using ICommand = BuildingBlocks.Abstractions.Commands.ICommand;

namespace FoodDelivery.Services.Identity.Identity.Features.RevokingRefreshToken.v1;

public record RevokeRefreshToken(string RefreshToken) : ICommand
{
    public static RevokeRefreshToken Of(string? refreshToken) => new(refreshToken.NotBeEmptyOrNull());
}

public class RevokeRefreshTokenHandler(IdentityContext context)
    : BuildingBlocks.Abstractions.Commands.ICommandHandler<RevokeRefreshToken>
{
    public async ValueTask<Unit> Handle(RevokeRefreshToken command, CancellationToken cancellationToken)
    {
        command.NotBeNull();

        var refreshToken = await context
            .Set<FoodDelivery.Services.Identity.Shared.Models.RefreshToken>()
            .FirstOrDefaultAsync(x => x.Token == command.RefreshToken, cancellationToken: cancellationToken);

        if (refreshToken == null)
            throw new RefreshTokenNotFoundException(refreshToken);

        if (!refreshToken.IsRefreshTokenValid())
            throw new InvalidRefreshTokenException(refreshToken);

        // revoke token and save
        refreshToken.RevokedAt = DateTime.Now;
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
