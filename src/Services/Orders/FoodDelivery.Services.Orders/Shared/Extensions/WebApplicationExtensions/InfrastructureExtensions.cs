using BuildingBlocks.Web.Cors;
using BuildingBlocks.Web.Minimal.Extensions;

namespace FoodDelivery.Services.Orders.Shared.Extensions.WebApplicationExtensions;

public static partial class WebApplicationExtensions
{
    public static void UseInfrastructure(this WebApplication app)
    {
        app.UseDefaultCors();

        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/security
        app.UseAuthentication();
        app.UseAuthorization();

        // map registered minimal endpoints
        app.MapMinimalEndpoints();
    }
}
