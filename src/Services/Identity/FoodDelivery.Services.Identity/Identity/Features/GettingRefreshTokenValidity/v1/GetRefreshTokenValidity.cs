using BuildingBlocks.Abstractions.Queries;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Identity.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Identity.Identity.Features.GettingRefreshTokenValidity.v1;

public record GetRefreshTokenValidity(Guid UserId, string RefreshToken) : IQuery<bool>
{
    public static GetRefreshTokenValidity Of(Guid userId, string? refreshToken) =>
        new(userId.NotBeEmpty(), refreshToken.NotBeNull());
}

public class GetRefreshTokenValidityQueryHandler(IdentityContext context) : IQueryHandler<GetRefreshTokenValidity, bool>
{
    public async ValueTask<bool> Handle(GetRefreshTokenValidity request, CancellationToken cancellationToken)
    {
        var refreshToken = await context
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
