using BuildingBlocks.Core.Extensions;
using FoodDelivery.BlazorWebApp.Contracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace FoodDelivery.BlazorWebApp.Services;

public class AccountsService(IHttpContextAccessor httpContextAccessor) : IAccountsService
{
    public Task Login(string? returnUrl)
    {
        httpContextAccessor.HttpContext.NotBeNull();

        var redirectUri = "/";

        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            if (IsLocalUrl(returnUrl))
            {
                redirectUri = returnUrl;
            }
        }

        var props = new AuthenticationProperties { RedirectUri = redirectUri };

        return httpContextAccessor.HttpContext.ChallengeAsync(props);
    }

    public async Task LogOutAsync(HttpContext httpContext)
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await httpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
    }

    private bool IsLocalUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return false;

        // Allows:
        // - Relative URLs (/path or ~/path)
        // - Absolute URLs that are empty or just contain a fragment (#)
        if (
            url.StartsWith("/", StringComparison.Ordinal)
            || url.StartsWith("~/", StringComparison.Ordinal)
            || url.StartsWith("#", StringComparison.Ordinal)
        )
        {
            return true;
        }

        // Disallows:
        // - Absolute URLs (http://...)
        // - URLs starting with \\
        // - URLs with a scheme (mailto:, tel:, etc.)
        if (Uri.TryCreate(url, UriKind.Absolute, out var absoluteUri))
        {
            return false;
        }

        return true; // Treat other cases as local
    }
}
