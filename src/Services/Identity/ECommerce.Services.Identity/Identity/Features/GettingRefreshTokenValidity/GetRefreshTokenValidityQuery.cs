using BuildingBlocks.Abstractions.CQRS.Queries;
using ECommerce.Services.Identity.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services.Identity.Identity.Features.GettingRefreshTokenValidity;

public record GetRefreshTokenValidityQuery(Guid UserId, string RefreshToken) : IQuery<bool>;

public class GetRefreshTokenValidityQueryHandler : IQueryHandler<GetRefreshTokenValidityQuery, bool>
{
    private readonly IdentityContext _context;

    public GetRefreshTokenValidityQueryHandler(IdentityContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(GetRefreshTokenValidityQuery request, CancellationToken cancellationToken)
    {
        var refreshToken = await _context.Set<Shared.Models.RefreshToken>()
            .FirstOrDefaultAsync(
                rt => rt.UserId == request.UserId &&
                      rt.Token == request.RefreshToken,
                cancellationToken);

        if (refreshToken == null)
        {
            return false;
        }

        if (!refreshToken.IsRefreshTokenValid())
            return false;

        return true;
    }
}
