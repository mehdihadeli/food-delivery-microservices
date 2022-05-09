using System.Globalization;
using Microsoft.AspNetCore.Identity;

namespace Store.Services.Identity.Shared.Models;

public class ApplicationRole : IdentityRole<Guid>
{
    public static ApplicationRole User => new()
    {
        Name = Constants.Role.User, NormalizedName = nameof(User).ToUpper(CultureInfo.InvariantCulture),
    };

    public static ApplicationRole Admin => new()
    {
        Name = Constants.Role.Admin,
        NormalizedName = nameof(Admin).ToUpper(CultureInfo.InvariantCulture)
    };
}
