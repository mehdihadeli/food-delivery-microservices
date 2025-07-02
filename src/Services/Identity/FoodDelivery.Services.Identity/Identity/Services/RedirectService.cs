using System.Text.RegularExpressions;

namespace FoodDelivery.Services.Identity.Identity.Services;

public class RedirectService : IRedirectService
{
    public string ExtractRedirectUriFromReturnUrl(string url)
    {
        var decodedUrl = System.Net.WebUtility.HtmlDecode(url);
        var results = Regex.Split(decodedUrl, "redirect_uri=");
        if (results.Length < 2)
            return "";

        string result = results[1];

        string splitKey;
        if (result.Contains("signin-oidc", StringComparison.InvariantCulture))
            splitKey = "signin-oidc";
        else
            splitKey = "scope";

        results = Regex.Split(result, splitKey);
        if (results.Length < 2)
            return "";

        result = results[0];

        return result
            .Replace("%3A", ":", StringComparison.InvariantCulture)
            .Replace("%2F", "/", StringComparison.InvariantCulture)
            .Replace("&", "", StringComparison.InvariantCulture);
    }
}
