using Microsoft.AspNetCore.Builder;

namespace BuildingBlocks.Web.Middlewares.RequestLogContextMiddleware;

public static class RequestLogContextMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogContextMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLogContextMiddlewareImp>();
    }
}
