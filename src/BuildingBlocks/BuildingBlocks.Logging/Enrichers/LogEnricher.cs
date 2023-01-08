using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Events;

namespace BuildingBlocks.Logging;

// Ref: https://andrewlock.net/using-serilog-aspnetcore-in-asp-net-core-3-logging-the-selected-endpoint-name-with-serilog/
// https://andrewlock.net/using-serilog-aspnetcore-in-asp-net-core-3-excluding-health-check-endpoints-from-serilog-request-logging/
// https://github.com/serilog/serilog-aspnetcore/issues/163
public static class LogEnricher
{
    /// <summary>
    /// Enriches the HTTP request log with additional data via the Diagnostic Context
    /// </summary>
    /// <param name="diagnosticContext">The Serilog diagnostic context</param>
    /// <param name="httpContext">The current HTTP Context</param>
    public static void EnrichFromRequest(IDiagnosticContext diagnosticContext, HttpContext httpContext)
    {
        var request = httpContext.Request;

        // Set all the common properties available for every request
        diagnosticContext.Set("Host", request.Host);
        diagnosticContext.Set("Protocol", request.Protocol);
        diagnosticContext.Set("Scheme", request.Scheme);

        // Only set it if available. You're not sending sensitive data in a querystring right?!
        if (request.QueryString.HasValue)
        {
            diagnosticContext.Set("QueryString", request.QueryString.Value);
        }

        // Set the content-type of the Response at this point
        diagnosticContext.Set("ContentType", httpContext.Response.ContentType);

        // Set userId
        diagnosticContext.Set(
            "UserId",
            httpContext.User.FindFirst(x => x.Type == ClaimTypes.NameIdentifier)?.Value);

        // Retrieve the IEndpointFeature selected for the request
        var endpoint = httpContext.GetEndpoint();
        if (endpoint is not null)
        {
            diagnosticContext.Set("EndpointName", endpoint.DisplayName);
        }
    }

    /// <summary>
    /// Shows request logs just in Information log level, this means we should set serilog `MinimumLevel:Default` to `Information` (for example ir doesn't show in Warning level) and shows health check logs just in `Debug` log level that is higher than Information (for getting fewer logs in the output).
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="_"></param>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static LogEventLevel GetLogLevel(HttpContext ctx, double _, Exception? ex) =>
        ex != null
            ? LogEventLevel.Error
            : ctx.Response.StatusCode > 499
                ? LogEventLevel.Error
                : IsHealthCheckEndpoint(ctx) || IsSwagger(ctx)
                    ? LogEventLevel.Debug
                    : LogEventLevel.Information;

    private static bool IsSwagger(HttpContext ctx)
    {
        var isHealth = ctx.Request.Path.Value?.Contains("swagger", StringComparison.Ordinal) ?? false;

        return isHealth;
    }

    private static bool IsHealthCheckEndpoint(HttpContext ctx)
    {
        var isHealth = ctx.Request.Path.Value?.Contains("health", StringComparison.Ordinal) ?? false;

        return isHealth;
    }
}
