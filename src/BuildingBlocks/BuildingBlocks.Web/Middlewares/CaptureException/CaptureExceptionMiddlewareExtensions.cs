using Microsoft.AspNetCore.Builder;

namespace BuildingBlocks.Web.Middlewares.CaptureException;

public static class CaptureExceptionMiddlewareExtensions
{
    // https://github.com/dotnet/aspnetcore/issues/4765
    // https://github.com/dotnet/aspnetcore/pull/47760
    public static IApplicationBuilder UseCaptureException(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.Properties["analysis.NextMiddlewareName"] = "Shared.Web.Middlewares.CaptureExceptionMiddleware";
        return app.UseMiddleware<CaptureExceptionMiddlewareImp>();
    }
}
