using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using FoodDelivery.Services.Identity.Shared.Extensions;
using FoodDelivery.Services.Identity.Shared.Models;
using IdentityModel;
using Microsoft.AspNetCore.Identity;

namespace FoodDelivery.Services.Identity.Identity.Services;

/// <summary>
/// Generating token claims for identity server based on identity user
/// </summary>
/// <param name="claimsFactory"></param>
/// <param name="userManager"></param>
public class IdentityProfileService(
    IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
    UserManager<ApplicationUser> userManager
) : IProfileService
{
    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var sub = context.Subject.GetSubjectId();
        var user = await userManager.FindByIdAsync(sub);

        if (user == null)
        {
            throw new Exception("User not found");
        }

        // Will add all Claims, RoleClaims and role to the claim list
        var principal = await claimsFactory.CreateAsync(user);
        var userClaims = principal.Claims.ToList();

        userClaims = userClaims.GroupBy(c => new { c.Type, c.Value }).Select(g => g.First()).ToList();

        // Get all requested scopes
        var requestedScopesValue = context.RequestedResources.RawScopeValues;
        var requestedScopesClaimTypes = context
            .RequestedResources.Resources.ApiScopes.SelectMany(x => x.UserClaims)
            .Union(context.RequestedResources.Resources.ApiResources.SelectMany(x => x.UserClaims))
            .Union(context.RequestedResources.Resources.IdentityResources.SelectMany(x => x.UserClaims));

        var requestedClaims = userClaims.Where(x => GetBasicClaims().Contains(x.Type)).ToList();
        requestedClaims.AddRangeIfNotExists(userClaims.Where(x => requestedScopesClaimTypes.Contains(x.Type)).ToList());

        // Only return claims that were explicitly requested via scopes
        context.IssuedClaims = requestedClaims;
    }

    private static IList<string> GetBasicClaims()
    {
        return new List<string> { JwtClaimTypes.Subject, JwtClaimTypes.Name, JwtClaimTypes.Email };
    }

    public async Task IsActiveAsync(IsActiveContext context)
    {
        var sub = context.Subject.GetSubjectId();
        var user = await userManager.FindByIdAsync(sub);
        context.IsActive = user != null;
    }
}
