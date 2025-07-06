using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.HealthCheck;

public static class WebApplicationExtensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";
    private const string HealthChecks = nameof(HealthChecks);

    public static WebApplication MapDefaultHealthChecks(this WebApplication app)
    {
        // Adding health checks endpoints to applications in non-development environments has security implications.
        // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
        if (app.Environment.IsDevelopment())
        {
            // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/health-checks#non-development-environments
            var healthChecks = app.MapGroup("");

            // Configure health checks endpoints to use the configured request timeouts and cache policies
            healthChecks.CacheOutput(HealthChecks).WithRequestTimeout(HealthChecks);

            // All health checks must pass for app to be considered ready to accept traffic after starting
            app.MapHealthChecks(HealthEndpointPath);

            // Only health checks tagged with the "live" tag must pass for app to be considered alive
            app.MapHealthChecks(
                AlivenessEndpointPath,
                new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("live"),
                }
            );

            // https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/#healthcheckui
            app.MapHealthChecksUI();
        }

        return app;
    }
}
