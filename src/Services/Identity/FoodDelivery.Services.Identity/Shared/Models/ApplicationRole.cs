using System.Globalization;
using Microsoft.AspNetCore.Identity;

namespace FoodDelivery.Services.Identity.Shared.Models;

public class ApplicationRole : IdentityRole<Guid>
{
    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = default!;
    public static ApplicationRole User =>
        new()
        {
            Name = IdentityConstants.Role.User,
            NormalizedName = nameof(User).ToUpper(CultureInfo.InvariantCulture),
        };

    public static ApplicationRole Admin =>
        new()
        {
            Name = IdentityConstants.Role.Admin,
            NormalizedName = nameof(Admin).ToUpper(CultureInfo.InvariantCulture),
        };
}
