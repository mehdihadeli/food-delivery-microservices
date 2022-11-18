using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Commands;
using ECommerce.Services.Identity.Identity.Exceptions;
using ECommerce.Services.Identity.Identity.Features.RefreshingToken.v1;
using ECommerce.Services.Identity.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services.Identity.Identity.Features.RevokingRefreshToken.v1;

public record RevokeRefreshToken(string RefreshToken) : ICommand;

internal class RevokeRefreshTokenHandler : ICommandHandler<RevokeRefreshToken>
{
    private readonly IdentityContext _context;

    public RevokeRefreshTokenHandler(IdentityContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(
        RevokeRefreshToken request,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(request, nameof(RevokeRefreshToken));

        var refreshToken = await _context.Set<global::ECommerce.Services.Identity.Shared.Models.RefreshToken>()
            .FirstOrDefaultAsync(x => x.Token == request.RefreshToken, cancellationToken: cancellationToken);

        if (refreshToken == null)
            throw new RefreshTokenCustomNotFoundException(refreshToken);

        if (!refreshToken.IsRefreshTokenValid())
            throw new InvalidRefreshTokenException(refreshToken);

        // revoke token and save
        refreshToken.RevokedAt = DateTime.Now;
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
