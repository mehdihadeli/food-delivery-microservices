using BuildingBlocks.Web.Cors;
using BuildingBlocks.Web.Minimal.Extensions;

namespace FoodDelivery.Services.Orders.Shared.Extensions.WebApplicationExtensions;

public static partial class WebApplicationExtensions
{
    public static void UseInfrastructure(this WebApplication app)
    {
        // Reads standard forwarded headers (X-Forwarded-For, X-Forwarded-Proto, X-Forwarded-Host) and updates the request information accordingly,
        // Ensures the application sees the original client IP, protocol (HTTP/HTTPS), and host rather than the proxy's information
        app.UseForwardedHeaders();

        app.UseDefaultCors();

        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/security
        app.UseAuthentication();
        app.UseAuthorization();

        // map registered minimal endpoints
        app.MapMinimalEndpoints();
    }
}
