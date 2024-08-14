using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Web.Middlewares.CaptureException;

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/write
public class CaptureExceptionMiddlewareImp(RequestDelegate next, ILogger<CaptureExceptionMiddlewareImp> logger)
{
    private readonly ILogger<CaptureExceptionMiddlewareImp> _logger = logger;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
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
