using BuildingBlocks.Web.Cors;
using BuildingBlocks.Web.Minimal.Extensions;

namespace FoodDelivery.Services.Identity.Shared.Extensions.WebApplicationExtensions;

public static class WebApplicationExtensions
{
    public static void UseInfrastructure(this WebApplication app)
    {
        // Reads standard forwarded headers (X-Forwarded-For, X-Forwarded-Proto, X-Forwarded-Host) and updates the request information accordingly,
        // Ensures the application sees the original client IP, protocol (HTTP/HTTPS), and host rather than the proxy's information
        app.UseForwardedHeaders();
        app.Use(
            (context, next) =>
            {
                // Handles the custom X-Forwarded-Prefix header that YARP is setting.Sets the PathBase property on the request so the application generates correct URLs.
                // Without this, URL generation might not include the original path prefix (/app, /auth).
                if (
                    context.Request.Headers.TryGetValue("X-Forwarded-Prefix", out var prefix)
                    && prefix.ToString().StartsWith('/')
                )
                {
                    context.Request.PathBase = prefix.ToString().TrimEnd('/');
                }

                return next();
            }
        );

        app.UseStaticFiles();

        app.UseDefaultCors();

        // This cookie policy fixes login issues with Chrome 80+ using HTTP
        app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Lax });
        app.UseIdentityServer();

        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/security
        app.UseAuthentication();
        app.UseAuthorization();

        // map registered minimal endpoints
        app.MapMinimalEndpoints();

        app.MapRazorPages().RequireAuthorization();
    }
}
