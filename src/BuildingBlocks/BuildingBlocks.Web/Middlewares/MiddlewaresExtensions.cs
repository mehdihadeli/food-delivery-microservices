using Microsoft.AspNetCore.Builder;

namespace BuildingBlocks.Web.Middlewares;

public static class MiddlewaresExtensions
{
    public static IApplicationBuilder UseRequestLogContextMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLogContextMiddleware>();
    }
}
