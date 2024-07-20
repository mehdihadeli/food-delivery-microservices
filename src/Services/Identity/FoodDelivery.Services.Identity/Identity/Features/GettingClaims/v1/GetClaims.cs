using BuildingBlocks.Abstractions.CQRS.Queries;

namespace FoodDelivery.Services.Identity.Identity.Features.GettingClaims.v1;

internal record GetClaims : IQuery<GetClaimsResult>
{
    public static GetClaims Of() => new();
}

internal class GetClaimsQueryHandler : IQueryHandler<GetClaims, GetClaimsResult>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetClaimsQueryHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<GetClaimsResult> Handle(GetClaims request, CancellationToken cancellationToken)
    {
        var claims = _httpContextAccessor.HttpContext?.User.Claims.Select(
            x => new ClaimDto { Type = x.Type, Value = x.Value }
        );

        return Task.FromResult(new GetClaimsResult(claims));
    }
}

internal record GetClaimsResult(IEnumerable<ClaimDto>? Claims);
