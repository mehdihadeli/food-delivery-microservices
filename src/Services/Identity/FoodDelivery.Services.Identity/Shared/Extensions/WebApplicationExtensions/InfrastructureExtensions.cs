using BuildingBlocks.Web.Cors;
using BuildingBlocks.Web.Minimal.Extensions;

namespace FoodDelivery.Services.Identity.Shared.Extensions.WebApplicationExtensions;

public static class WebApplicationExtensions
{
    public static void UseInfrastructure(this WebApplication app)
    {
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
