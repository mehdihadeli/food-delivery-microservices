using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Identity.Identity.Exceptions;
using FoodDelivery.Services.Identity.Identity.Features.RefreshingToken.v1;
using FoodDelivery.Services.Identity.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Identity.Identity.Features.RevokingRefreshToken.V1;

internal record RevokeRefreshToken(string RefreshToken) : ICommand
{
    public static RevokeRefreshToken Of(string? refreshToken) => new(refreshToken.NotBeEmptyOrNull());
}

internal class RevokeRefreshTokenHandler : ICommandHandler<RevokeRefreshToken>
{
    private readonly IdentityContext _context;

    public RevokeRefreshTokenHandler(IdentityContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(RevokeRefreshToken command, CancellationToken cancellationToken)
    {
        command.NotBeNull();

        var refreshToken = await _context
            .Set<global::FoodDelivery.Services.Identity.Shared.Models.RefreshToken>()
            .FirstOrDefaultAsync(x => x.Token == command.RefreshToken, cancellationToken: cancellationToken);

        if (refreshToken == null)
            throw new RefreshTokenNotFoundException(refreshToken);

        if (!refreshToken.IsRefreshTokenValid())
            throw new InvalidRefreshTokenException(refreshToken);

        // revoke token and save
        refreshToken.RevokedAt = DateTime.Now;
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
