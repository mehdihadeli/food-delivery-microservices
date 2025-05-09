using FoodDelivery.Services.Shared.Identity.Users;
using Microsoft.AspNetCore.Identity;

namespace FoodDelivery.Services.Identity.Shared.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public DateTime? LastLoggedInAt { get; set; }

    // .Net identity navigations -> https://docs.microsoft.com/en-us/aspnet/core/security/authentication/customize-identity-model#add-navigation-properties
    // Only Used by DbContext
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = default!;
    public virtual ICollection<AccessToken> AccessTokens { get; set; } = default!;
    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = default!;

    public virtual ICollection<ApplicationUserClaim> UserClaims { get; set; } = new List<ApplicationUserClaim>();
    public UserState UserState { get; set; }
    public DateTime CreatedAt { get; set; }
}
