using BuildingBlocks.Web.Extensions;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace BuildingBlocks.Web.Middlewares;

public class RequestLogContextMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLogContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        using (LogContext.PushProperty("CorrelationId", context.GetCorrelationId()))
        {
            await _next.Invoke(context);
        }
    }
}
