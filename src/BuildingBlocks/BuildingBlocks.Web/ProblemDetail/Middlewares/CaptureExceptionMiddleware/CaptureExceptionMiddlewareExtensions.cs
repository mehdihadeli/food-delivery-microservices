using Microsoft.AspNetCore.Builder;

namespace BuildingBlocks.Web.ProblemDetail.Middlewares.CaptureExceptionMiddleware;

//https://github.com/dotnet/aspnetcore/pull/47760
public static class CaptureExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseCaptureException(this IApplicationBuilder app)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        app.Properties["analysis.NextMiddlewareName"] =
            "Vertical.Slice.Template.Shared.Web.Middlewares.CaptureExceptionMiddleware";
        return app.UseMiddleware<CaptureExceptionMiddlewareImp>();
    }
}
