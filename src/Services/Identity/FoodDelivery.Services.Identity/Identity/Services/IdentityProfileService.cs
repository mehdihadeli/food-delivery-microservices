using System.Security.Claims;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using FoodDelivery.Services.Identity.Shared.Models;
using IdentityModel;
using Microsoft.AspNetCore.Identity;

namespace FoodDelivery.Services.Identity.Identity.Services;

public class IdentityProfileService(
    IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
    UserManager<ApplicationUser> userManager
) : IProfileService
{
    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var sub = context.Subject.GetSubjectId();
        var user = await userManager.FindByIdAsync(sub);
        var roles = await userManager.GetRolesAsync(user);
        var isAdmin = roles.Contains(IdentityConstants.Role.Admin);
        var principal = await claimsFactory.CreateAsync(user);

        var claims = principal.Claims.ToList();
        claims = claims.Where(claim => context.RequestedClaimTypes.Contains(claim.Type)).ToList();

        claims.Add(new Claim(JwtClaimTypes.Id, user.Id.ToString()));
        claims.Add(new Claim(JwtClaimTypes.Name, user.UserName));
        claims.Add(new Claim(JwtClaimTypes.Email, user.Email));

        claims.Add(isAdmin ? new Claim(JwtClaimTypes.Role, "admin") : new Claim(JwtClaimTypes.Role, "user"));

        context.IssuedClaims = claims;
    }

    public async Task IsActiveAsync(IsActiveContext context)
    {
        var sub = context.Subject.GetSubjectId();
        var user = await userManager.FindByIdAsync(sub);
        context.IsActive = user != null;
    }
}
