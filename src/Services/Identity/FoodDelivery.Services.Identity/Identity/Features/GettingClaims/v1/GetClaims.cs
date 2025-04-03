using BuildingBlocks.Abstractions.Queries;

namespace FoodDelivery.Services.Identity.Identity.Features.GettingClaims.v1;

public record GetClaims : IQuery<GetClaimsResult>
{
    public static GetClaims Of() => new();
}

public class GetClaimsQueryHandler(IHttpContextAccessor httpContextAccessor) : IQueryHandler<GetClaims, GetClaimsResult>
{
    public ValueTask<GetClaimsResult> Handle(GetClaims request, CancellationToken cancellationToken)
    {
        var claims = httpContextAccessor.HttpContext?.User.Claims.Select(x => new ClaimDto
        {
            Type = x.Type,
            Value = x.Value,
        });

        return ValueTask.FromResult(new GetClaimsResult(claims));
    }
}

public record GetClaimsResult(IEnumerable<ClaimDto>? Claims);
