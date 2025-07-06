using System.Security.Claims;

namespace FoodDelivery.Services.Identity.Shared.Extensions;

public static class ClaimExtensions
{
    public static void AddIfNotExists(this IList<Claim> claims, Claim newClaim)
    {
        if (!claims.Any(c => c.Type == newClaim.Type && c.Value == newClaim.Value))
        {
            claims.Add(newClaim);
        }
    }

    public static void AddRangeIfNotExists(this IList<Claim> claims, IList<Claim> newClaims)
    {
        foreach (var newClaim in newClaims)
        {
            AddIfNotExists(claims, newClaim);
        }
    }
}
