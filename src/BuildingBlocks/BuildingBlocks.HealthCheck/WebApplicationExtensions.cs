using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Prometheus;

namespace BuildingBlocks.HealthCheck;

public static class WebApplicationExtensions
{
    public static WebApplication MapCustomHealthChecks(this WebApplication app)
    {
        // Adding health checks endpoints to applications in non-development environments has security implications.
        // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
        if (app.Environment.IsDevelopment())
        {
            // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/health-checks#non-development-environments
            var healthChecks = app.MapGroup("");

            healthChecks.CacheOutput("HealthChecks").WithRequestTimeout("HealthChecks");

            // All health checks must pass for app to be
            // considered ready to accept traffic after starting
            healthChecks.MapHealthChecks("/health");

            // Only health checks tagged with the "live" tag
            // must pass for app to be considered alive
            healthChecks.MapHealthChecks("/alive", new() { Predicate = static r => r.Tags.Contains("live") });

            healthChecks.MapHealthChecks(
                "ready",
                new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") }
            );

            healthChecks.MapHealthChecks(
                "/health/infra",
                new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("infra"),
                    AllowCachingResponses = false,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                }
            );

            healthChecks.MapHealthChecks(
                "/health/bus",
                new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("bus"),
                    AllowCachingResponses = false,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                }
            );

            healthChecks.MapHealthChecks(
                "/health/database",
                new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("database"),
                    AllowCachingResponses = false,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                }
            );

            healthChecks.MapHealthChecks(
                "/health/downstream-services",
                new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("downstream-services"),
                    AllowCachingResponses = false,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                }
            );

            // export health metrics in `/health/metrics` endpoint and should scrape in the Prometheus config file and `scrape_configs` beside of `/metrics` endpoint for application metrics
            app.UseHealthChecksPrometheusExporter(
                "/health/metrics",
                options =>
                {
                    options.ResultStatusCodes[HealthStatus.Unhealthy] = 200;
                }
            );

            app.UseHealthChecksUI(setup =>
            {
                setup.ApiPath = "/healthcheck";
                setup.UIPath = "/healthcheck-ui";
            });
        }

        return app;
    }
}
