using Microsoft.AspNetCore.Authentication;

namespace FoodDelivery.Services.Identity.Identity.Services;

public interface ILoginService<T>
{
    Task<bool> ValidateCredentials(T user, string password);

    Task<T> FindByUsername(string user);

    Task SignIn(T user);

    Task SignInAsync(T user, AuthenticationProperties properties, string authenticationMethod = null);
}
