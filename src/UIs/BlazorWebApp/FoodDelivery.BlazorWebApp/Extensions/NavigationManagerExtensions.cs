using Microsoft.AspNetCore.Components;

namespace FoodDelivery.BlazorWebApp.Extensions;

public static class NavigationManagerExtensions
{
    public static HttpContext GetHttpContext(this NavigationManager navigationManager)
    {
        var context = new HttpContextAccessor().HttpContext;
        if (context == null)
        {
            throw new InvalidOperationException("HttpContext cannot be accessed here");
        }

        return context;
    }
}
