namespace FoodDelivery.BlazorWebApp.Contracts;

public interface IAccountsService
{
    Task Login(string? returnUrl);
    Task LogOutAsync(HttpContext httpContext);
}
