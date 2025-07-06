using FoodDelivery.Services.Identity.Shared.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace FoodDelivery.Services.Identity.Identity.Services;

public class EfLoginService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    : ILoginService<ApplicationUser>
{
    public async Task<ApplicationUser> FindByUsername(string user)
    {
        return await userManager.FindByEmailAsync(user);
    }

    public async Task<bool> ValidateCredentials(ApplicationUser user, string password)
    {
        return await userManager.CheckPasswordAsync(user, password);
    }

    public Task SignIn(ApplicationUser user)
    {
        return signInManager.SignInAsync(user, true);
    }

    public Task SignInAsync(
        ApplicationUser user,
        AuthenticationProperties properties,
        string authenticationMethod = null
    )
    {
        return signInManager.SignInAsync(user, properties, authenticationMethod);
    }
}
