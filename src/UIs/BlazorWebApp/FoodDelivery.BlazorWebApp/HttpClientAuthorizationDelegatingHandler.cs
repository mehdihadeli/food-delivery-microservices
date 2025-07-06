using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;

namespace FoodDelivery.BlazorWebApp;

// Also we can use https://docs.duendesoftware.com/accesstokenmanagement/web-apps/
public class HttpClientAuthorizationDelegatingHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        // Try to get the token from the current authentication session (cookie), in balazor server app we should not have a token in Authorization header
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var token = await httpContext.GetTokenAsync("access_token");
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
