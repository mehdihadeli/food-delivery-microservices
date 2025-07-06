using FoodDelivery.Services.Shared.Identity.Users;
using Microsoft.AspNetCore.Identity;

namespace FoodDelivery.Services.Identity.Shared.Models;

// - We use UserManager and RoleManager to access ApplicationUserRole and ApplicationUserClaim, Removing the navigation properties (UserRoles --> List<ApplicationUserRole> and UserClaims --> List<ApplicationUserClaim>) from ApplicationUser can simplify
// our model and prevent accidental performance issues like unwanted lazy loading or over-fetching data. Since UserManager and RoleManager already provide methods (GetRolesAsync(), GetClaimsAsync()) to access this data, explicit navigation properties aren’t strictly necessary.
public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public DateTime? LastLoggedInAt { get; set; }
    public UserState UserState { get; set; }
    public DateTime CreatedAt { get; set; }
}
