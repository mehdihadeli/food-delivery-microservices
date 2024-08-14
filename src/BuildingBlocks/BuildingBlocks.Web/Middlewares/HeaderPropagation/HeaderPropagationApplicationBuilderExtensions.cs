using BuildingBlocks.Core.Web.HeaderPropagation;
using Microsoft.AspNetCore.Builder;

namespace BuildingBlocks.Web.Middlewares.HeaderPropagation;

public static class HeaderPropagationApplicationBuilderExtensions
{
    public static IApplicationBuilder UseHeaderPropagation(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (app.ApplicationServices.GetService<HeaderPropagationStore>() == null)
        {
            throw new InvalidOperationException(
                "CustomHeaderPropagationStore not registered. Please add it with AddHeaderPropagation"
            );
        }

        return app.UseMiddleware<HeaderPropagationMiddleware>();
    }
}
