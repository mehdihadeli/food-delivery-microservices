using BuildingBlocks.Abstractions.Queries;

namespace FoodDelivery.Services.Identity.Identity.Features.GettingClaims.v1;

internal record GetClaims : IQuery<GetClaimsResult>
{
    public static GetClaims Of() => new();
}

internal class GetClaimsQueryHandler(IHttpContextAccessor httpContextAccessor)
    : IQueryHandler<GetClaims, GetClaimsResult>
{
    public Task<GetClaimsResult> Handle(GetClaims request, CancellationToken cancellationToken)
    {
        var claims = httpContextAccessor.HttpContext?.User.Claims.Select(x => new ClaimDto
        {
            Type = x.Type,
            Value = x.Value
        });

        return Task.FromResult(new GetClaimsResult(claims));
    }
}

internal record GetClaimsResult(IEnumerable<ClaimDto>? Claims);
