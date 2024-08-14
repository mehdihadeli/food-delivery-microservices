using BuildingBlocks.Abstractions.Queries;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Identity.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Identity.Identity.Features.GettingRefreshTokenValidity.v1;

internal record GetRefreshTokenValidity(Guid UserId, string RefreshToken) : IQuery<bool>
{
    public static GetRefreshTokenValidity Of(Guid userId, string? refreshToken) =>
        new(userId.NotBeEmpty(), refreshToken.NotBeNull());
}

internal class GetRefreshTokenValidityQueryHandler : IQueryHandler<GetRefreshTokenValidity, bool>
{
    private readonly IdentityContext _context;

    public GetRefreshTokenValidityQueryHandler(IdentityContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(GetRefreshTokenValidity request, CancellationToken cancellationToken)
    {
        var refreshToken = await _context
            .Set<Shared.Models.RefreshToken>()
            .FirstOrDefaultAsync(
                rt => rt.UserId == request.UserId && rt.Token == request.RefreshToken,
                cancellationToken
            );

        if (refreshToken == null)
        {
            return false;
        }

        return refreshToken.IsRefreshTokenValid();
    }
}
