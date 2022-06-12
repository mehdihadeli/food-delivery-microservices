using System.Security.Claims;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using ECommerce.Services.Identity.Shared.Models;
using IdentityModel;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Services.Identity.Identity.Services;

public class IdentityProfileService : IProfileService
{
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _claimsFactory;
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityProfileService(
        IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
        UserManager<ApplicationUser> userManager)
    {
        _claimsFactory = claimsFactory;
        _userManager = userManager;
    }

    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var sub = context.Subject.GetSubjectId();
        var user = await _userManager.FindByIdAsync(sub);
        var roles = await _userManager.GetRolesAsync(user);
        var isAdmin = roles.Contains(IdentityConstants.Role.Admin);
        var principal = await _claimsFactory.CreateAsync(user);

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
        var user = await _userManager.FindByIdAsync(sub);
        context.IsActive = user != null;
    }
}
