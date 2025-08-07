using System.Net;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.AspireIntegrations.Prometheus;

internal sealed class PrometheusHealthCheck(PrometheusResource resource) : IHealthCheck
{
    private readonly HttpClient _httpClient = new();

    // https://prometheus.io/docs/prometheus/latest/management_api/#health-check
    private readonly Uri _healthEndpoint = new(
        new Uri($"http://{resource.PrimaryEndpoint.Host}:{resource.PrimaryEndpoint.Port}"),
        "-/healthy"
    );

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var response = await _httpClient.GetAsync(_healthEndpoint, cancellationToken);

            if (response.StatusCode != HttpStatusCode.OK)
                return HealthCheckResult.Unhealthy($"Prometheus health check failed: Status {response.StatusCode}");

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return content.Contains("Prometheus Server is Healthy.", StringComparison.OrdinalIgnoreCase)
                ? HealthCheckResult.Healthy("Prometheus is healthy")
                : HealthCheckResult.Unhealthy($"Unexpected Prometheus health response: {content}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Failed to check Prometheus health status", ex);
        }
    }
}
