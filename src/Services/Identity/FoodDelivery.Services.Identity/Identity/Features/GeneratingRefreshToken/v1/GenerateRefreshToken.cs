using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Utils;
using FoodDelivery.Services.Identity.Identity.Dtos.v1;
using FoodDelivery.Services.Identity.Identity.Features.RefreshingToken.v1;
using FoodDelivery.Services.Identity.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Identity.Identity.Features.GeneratingRefreshToken.v1;

internal record GenerateRefreshToken(Guid UserId, string? Token = null) : ICommand<GenerateRefreshTokenResult>
{
    public static GenerateRefreshToken Of(Guid userId, string? token = null) => new(userId.NotBeEmpty(), token);
}

internal class GenerateRefreshTokenHandler : ICommandHandler<GenerateRefreshToken, GenerateRefreshTokenResult>
{
    private readonly IdentityContext _context;

    public GenerateRefreshTokenHandler(IdentityContext context)
    {
        _context = context;
    }

    public async Task<GenerateRefreshTokenResult> Handle(
        GenerateRefreshToken command,
        CancellationToken cancellationToken
    )
    {
        command.NotBeNull();

        var refreshToken = await _context
            .Set<Shared.Models.RefreshToken>()
            .FirstOrDefaultAsync(rt => rt.UserId == command.UserId && rt.Token == command.Token, cancellationToken);

        if (refreshToken == null)
        {
            var token = Shared.Models.RefreshToken.GetRefreshToken();

            refreshToken = new Shared.Models.RefreshToken
            {
                UserId = command.UserId,
                Token = token,
                CreatedAt = DateTime.Now,
                ExpiredAt = DateTime.Now.AddDays(1),
                CreatedByIp = IpUtilities.GetIpAddress()
            };

            await _context.Set<Shared.Models.RefreshToken>().AddAsync(refreshToken, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            if (!refreshToken.IsRefreshTokenValid())
                throw new InvalidRefreshTokenException(refreshToken);

            var token = Shared.Models.RefreshToken.GetRefreshToken();

            refreshToken.Token = token;
            refreshToken.ExpiredAt = DateTime.Now;
            refreshToken.CreatedAt = DateTime.Now.AddDays(10);
            refreshToken.CreatedByIp = IpUtilities.GetIpAddress();

            _context.Set<Shared.Models.RefreshToken>().Update(refreshToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // remove old refresh tokens from user
        // we could also maintain them on the database with changing their revoke date
        await RemoveOldRefreshTokens(command.UserId);

        return new GenerateRefreshTokenResult(
            new RefreshTokenDto
            {
                Token = refreshToken.Token,
                CreatedAt = refreshToken.CreatedAt,
                ExpireAt = refreshToken.ExpiredAt,
                UserId = refreshToken.UserId,
                CreatedByIp = refreshToken.CreatedByIp,
                IsActive = refreshToken.IsActive,
                IsExpired = refreshToken.IsExpired,
                IsRevoked = refreshToken.IsRevoked,
                RevokedAt = refreshToken.RevokedAt
            }
        );
    }

    private Task RemoveOldRefreshTokens(Guid userId, long? ttlRefreshToken = null)
    {
        var refreshTokens = _context
            .Set<global::FoodDelivery.Services.Identity.Shared.Models.RefreshToken>()
            .Where(rt => rt.UserId == userId);

        refreshTokens.ToList().RemoveAll(x => !x.IsRefreshTokenValid(ttlRefreshToken));

        return _context.SaveChangesAsync();
    }
}

public record GenerateRefreshTokenResult(RefreshTokenDto RefreshToken);
