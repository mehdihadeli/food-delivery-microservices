namespace FoodDelivery.Services.Identity.Identity.Services;

public interface IRedirectService
{
    string ExtractRedirectUriFromReturnUrl(string url);
}
