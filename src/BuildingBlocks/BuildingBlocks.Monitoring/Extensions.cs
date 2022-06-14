using System.Text;
using System.Text.Json;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Prometheus;

namespace BuildingBlocks.Monitoring;

// https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks
// https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
// https://nikiforovall.github.io/dotnet/aspnetcore/coding-stories/2021/07/25/add-health-checks-to-aspnetcore.html
// https://code-maze.com/health-checks-aspnetcore/
// https://github.com/prometheus-net/prometheus-net
public static class Extensions
{
    public static IServiceCollection AddMonitoring(
        this IServiceCollection services,
        Action<IHealthChecksBuilder>? healthChecksBuilder = null)
    {
        var healCheckBuilder =
            services.AddHealthChecks()
                .ForwardToPrometheus();

        healthChecksBuilder?.Invoke(healCheckBuilder);

        //// health check ui has problem with .net 6
        services.AddHealthChecksUI(setup =>
        {
            setup.SetEvaluationTimeInSeconds(60); // time in seconds between check
            setup.AddHealthCheckEndpoint("All Checks", "/healthz");
            setup.AddHealthCheckEndpoint("Infra", "/health/infra");
            setup.AddHealthCheckEndpoint("Bus", "/health/bus");
            setup.AddHealthCheckEndpoint("Database", "/health/database");
            setup.AddHealthCheckEndpoint("Downstream Services", "/health/downstream-services");
        }).AddInMemoryStorage();

        return services;
    }

    public static IApplicationBuilder UseMonitoring(this IApplicationBuilder app)
    {
        app.UseHttpMetrics();
        app.UseGrpcMetrics();

        app.UseHealthChecks("/healthz",
                new HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                    ResultStatusCodes =
                    {
                        [HealthStatus.Healthy] = StatusCodes.Status200OK,
                        [HealthStatus.Degraded] = StatusCodes.Status500InternalServerError,
                        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
                    },
                })
            .UseHealthChecks("/health/infra",
                new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("infra"),
                    AllowCachingResponses = false,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                })
            .UseHealthChecks("/health/bus",
                new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("bus"),
                    AllowCachingResponses = false,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                })
            .UseHealthChecks("/health/database",
                new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("database"),
                    AllowCachingResponses = false,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                })
            .UseHealthChecks("/health/downstream-services",
                new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("downstream-services"),
                    AllowCachingResponses = false,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                })
            .UseHealthChecks("/health/ready",
                new HealthCheckOptions
                {
                    Predicate = _ => true, AllowCachingResponses = false, ResponseWriter = WriteResponseAsync,
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

        var options = new JsonWriterOptions {Indented = true};

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
        }

        var json = Encoding.UTF8.GetString(stream.ToArray());

        return context.Response.WriteAsync(json, default);
    }
}
