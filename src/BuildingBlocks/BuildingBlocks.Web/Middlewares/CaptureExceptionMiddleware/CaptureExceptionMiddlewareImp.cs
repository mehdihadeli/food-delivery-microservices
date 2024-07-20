using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Web.Middlewares.CaptureExceptionMiddleware;

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/write
public class CaptureExceptionMiddlewareImp
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CaptureExceptionMiddlewareImp> _logger;

    public CaptureExceptionMiddlewareImp(RequestDelegate next, ILogger<CaptureExceptionMiddlewareImp> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception e)
        {
            CaptureException(e, context);
            throw;
        }
    }

    private static void CaptureException(Exception exception, HttpContext context)
    {
        ExceptionHandlerFeature instance = new ExceptionHandlerFeature
        {
            Path = context.Request.Path,
            Error = exception
        };
        context.Features.Set<IExceptionHandlerPathFeature>((IExceptionHandlerPathFeature)instance);
        context.Features.Set<IExceptionHandlerFeature>((IExceptionHandlerFeature)instance);
    }
}
