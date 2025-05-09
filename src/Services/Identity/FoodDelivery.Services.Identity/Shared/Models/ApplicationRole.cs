using System.Globalization;
using Microsoft.AspNetCore.Identity;

namespace FoodDelivery.Services.Identity.Shared.Models;

public class ApplicationRole : IdentityRole<Guid>
{
    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = null!;
    public virtual ICollection<ApplicationRoleClaim> RoleClaims { get; set; } = new List<ApplicationRoleClaim>();

    public static ApplicationRole UserRole =>
        new()
        {
            Name = IdentityConstants.Role.User,
            NormalizedName = nameof(UserRole).ToUpper(CultureInfo.InvariantCulture),
        };

    public static ApplicationRole AdminRole =>
        new()
        {
            Name = IdentityConstants.Role.Admin,
            NormalizedName = nameof(AdminRole).ToUpper(CultureInfo.InvariantCulture),
        };
}
