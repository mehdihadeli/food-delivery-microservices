using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;

namespace BuildingBlocks.HealthCheck;

public static class WebApplicationExtensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";
    private const string HealthChecks = nameof(HealthChecks);

    public static WebApplication MapDefaultHealthChecks(this WebApplication app)
    {
        // ref: https://github.com/dotnet/aspire-samples/blob/main/samples/HealthChecksUI/HealthChecksUI.ServiceDefaults/Extensions.cs#L108

        // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/health-checks#non-development-environments
        var healthChecks = app.MapGroup("");

        // Configure health checks endpoints to use the configured request timeouts and cache policies
        healthChecks.CacheOutput(policyName: HealthChecks).WithRequestTimeout(policyName: HealthChecks);

        // All health checks must pass for app to be considered ready to accept traffic after starting
        healthChecks.MapHealthChecks(HealthEndpointPath);

        // Only health checks tagged with the "live" tag must pass for app to be considered alive
        healthChecks.MapHealthChecks(AlivenessEndpointPath, new() { Predicate = r => r.Tags.Contains("live") });

        // https://github.com/dotnet/aspire-samples/tree/main/samples/HealthChecksUI
        // Add the health checks endpoint for the HealthChecksUI on `/healthz`
        var healthChecksUrls = app.Configuration["HEALTHCHECKSUI_URLS"];
        if (!string.IsNullOrWhiteSpace(healthChecksUrls))
        {
            var pathToHostsMap = GetPathToHostsMap(healthChecksUrls);

            foreach (var path in pathToHostsMap.Keys)
            {
                // Ensure that the HealthChecksUI endpoint is only accessible from configured hosts, e.g. localhost:12345, hub.docker.internal, etc.
                // as it contains more detailed information about the health of the app including the types of dependencies it has.

                healthChecks
                    .MapHealthChecks(path, new() { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse })
                    // This ensures that the HealthChecksUI endpoint is only accessible from the configured health checks URLs.
                    // See this documentation to learn more about restricting access to health checks endpoints via routing:
                    // https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-8.0#use-health-checks-routing
                    .RequireHost(pathToHostsMap[path]);
            }
        }

        return app;
    }

    private static Dictionary<string, string[]> GetPathToHostsMap(string healthChecksUrls)
    {
        // Given a value like "localhost:12345/healthz;hub.docker.internal:12345/healthz" return a dictionary like:
        // { { "healthz", [ "localhost:12345", "hub.docker.internal:12345" ] } }

        var uris = healthChecksUrls
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(url => new Uri(url, UriKind.Absolute))
            .GroupBy(uri => uri.AbsolutePath, uri => uri.Authority)
            .ToDictionary(g => g.Key, g => g.ToArray());

        return uris;
    }
}
