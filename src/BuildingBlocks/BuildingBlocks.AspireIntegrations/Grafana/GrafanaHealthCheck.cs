using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.AspireIntegrations.Grafana;

internal sealed class GrafanaHealthCheck(GrafanaResource resource) : IHealthCheck
{
    private readonly HttpClient _httpClient = new();

    // https://grafana.com/docs/grafana/latest/developers/http_api/other/#health-api
    private readonly Uri _healthEndpoint = new(
        new Uri($"http://{resource.PrimaryEndpoint.Host}:{resource.PrimaryEndpoint.Port}"),
        "api/health"
    );

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var response = await _httpClient.GetAsync(_healthEndpoint, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Unhealthy(
                    $"Grafana health check failed with status code: {response.StatusCode}"
                );
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            var healthStatus = JsonSerializer.Deserialize<GrafanaHealthResponse>(
                content,
                CreateDefaultSerializerOptions()
            );

            return healthStatus?.Database == "ok" && !string.IsNullOrEmpty(healthStatus.Commit)
                ? HealthCheckResult.Healthy("Grafana is healthy and ready")
                : HealthCheckResult.Unhealthy($"Grafana health check failed: {content}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Failed to check Grafana health status", ex);
        }
    }

    private static JsonSerializerOptions CreateDefaultSerializerOptions(bool camelCase = true, bool indented = false)
    {
        var options = new JsonSerializerOptions
        {
            IncludeFields = true,
            PropertyNameCaseInsensitive = true,
            // Equivalent to ReferenceLoopHandling.Ignore
            ReferenceHandler = ReferenceHandler.IgnoreCycles,

            WriteIndented = indented,
            PropertyNamingPolicy = camelCase ? JsonNamingPolicy.CamelCase : null,
        };

        return options;
    }

    private record GrafanaHealthResponse(string Commit, string Database, string Version);
}
