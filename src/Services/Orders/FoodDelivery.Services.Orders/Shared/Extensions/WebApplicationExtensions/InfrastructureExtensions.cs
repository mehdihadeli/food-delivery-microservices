using CloudNativeKit.OpenApi.AspnetOpenApi.Extensions;
using CloudNativeKit.Web.Cors;
using CloudNativeKit.Web.Minimal.Extensions;

namespace FoodDelivery.Services.Orders.Shared.Extensions.WebApplicationExtensions;

public static partial class WebApplicationExtensions
{
    public static void UseInfrastructure(this WebApplication app)
    {
        app.UseAspnetOpenApi();

        app.UseDefaultCors();

        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/security
        app.UseAuthentication();
        app.UseAuthorization();

        // map registered minimal endpoints
        app.MapMinimalEndpoints();
    }
}
