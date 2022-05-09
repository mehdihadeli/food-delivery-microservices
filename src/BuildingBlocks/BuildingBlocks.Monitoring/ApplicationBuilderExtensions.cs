using System.Text;
using System.Text.Json;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.Monitoring;

// https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
// https://nikiforovall.github.io/dotnet/aspnetcore/coding-stories/2021/07/25/add-health-checks-to-aspnetcore.html
public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseCustomHealthCheck(this IApplicationBuilder app)
    {
        app.UseHealthChecks("/healthz",
                new HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                    ResultStatusCodes =
                    {
                        [HealthStatus.Healthy] = StatusCodes.Status200OK,
                        [HealthStatus.Degraded] = StatusCodes.Status500InternalServerError,
                        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                    }
                })
            .UseHealthChecks("/health",
                new HealthCheckOptions
                {
                    Predicate = check => !check.Tags.Contains("services"),
                    AllowCachingResponses = false,
                    ResponseWriter = WriteResponseAsync
                })
            .UseHealthChecks("/health/ready",
                new HealthCheckOptions
                {
                    Predicate = _ => true, AllowCachingResponses = false, ResponseWriter = WriteResponseAsync
                })
            .UseHealthChecksUI(setup =>
            {
                setup.ApiPath = "/healthcheck";
                setup.UIPath = "/healthcheck-ui";
            });

        return app;
    }

    private static Task WriteResponseAsync(HttpContext context, HealthReport result)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var options = new JsonWriterOptions { Indented = true };

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, options))
        {
            writer.WriteStartObject();
            writer.WriteString("status", result.Status.ToString());
            writer.WriteStartObject("results");
            foreach (var entry in result.Entries)
            {
                writer.WriteStartObject(entry.Key);
                writer.WriteString("status", entry.Value.Status.ToString());
                writer.WriteEndObject();
            }

            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        var json = Encoding.UTF8.GetString(stream.ToArray());

        return context.Response.WriteAsync(json, default);
    }
}
