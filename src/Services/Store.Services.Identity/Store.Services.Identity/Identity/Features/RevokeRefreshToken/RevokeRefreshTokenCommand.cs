using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Command;
using Store.Services.Identity.Identity.Exceptions;
using Store.Services.Identity.Identity.Features.RefreshingToken;
using Store.Services.Identity.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace Store.Services.Identity.Identity.Features.RevokeRefreshToken;

public record RevokeRefreshTokenCommand(string RefreshToken) : ICommand;

internal class RevokeRefreshTokenCommandHandler : ICommandHandler<RevokeRefreshTokenCommand>
{
    private readonly IdentityContext _context;

    public RevokeRefreshTokenCommandHandler(IdentityContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(
        RevokeRefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(request, nameof(RevokeRefreshTokenCommand));

        var refreshToken = await _context.Set<global::Store.Services.Identity.Shared.Models.RefreshToken>()
            .FirstOrDefaultAsync(x => x.Token == request.RefreshToken, cancellationToken: cancellationToken);

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
