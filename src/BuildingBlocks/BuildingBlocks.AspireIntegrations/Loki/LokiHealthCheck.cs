using System.Net;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.AspireIntegrations.Loki;

internal sealed class LokiHealthCheck(LokiResource resource) : IHealthCheck
{
    private readonly HttpClient _httpClient = new();

    // https://grafana.com/docs/tempo/latest/api_docs/#readiness-probe
    private readonly Uri _healthEndpoint = new(
        new Uri($"http://{resource.HttpEndpoint.Host}:{resource.HttpEndpoint.Port}"),
        "ready"
    );

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var response = await _httpClient.GetAsync(_healthEndpoint, cancellationToken);

            return response.StatusCode == HttpStatusCode.OK
                ? HealthCheckResult.Healthy("Loki is healthy and ready")
                : HealthCheckResult.Unhealthy("Loki health check failed");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Failed to check Loki health status", ex);
        }
    }
}
