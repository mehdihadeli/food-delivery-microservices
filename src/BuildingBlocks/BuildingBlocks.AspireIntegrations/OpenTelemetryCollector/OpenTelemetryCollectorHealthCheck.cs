using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.AspireIntegrations.OpenTelemetryCollector;

internal sealed class OpenTelemetryCollectorHealthCheck(OpenTelemetryCollectorResource resource) : IHealthCheck
{
    private readonly HttpClient _httpClient = new();
    private readonly Uri _healthEndpoint = new(
        $"http://{resource.HealthCheckEndpoint.Host}:{resource.HealthCheckEndpoint.Port}/"
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
                    $"OTel Collector health check failed with HTTP status: {response.StatusCode}"
                );
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var healthStatus = JsonSerializer.Deserialize<OtelCollectorHealthResponse>(
                content,
                CreateDefaultSerializerOptions()
            );

            return healthStatus?.Status == "Server available"
                ? HealthCheckResult.Healthy($"OTel Collector is available (up since: {healthStatus.UpSince})")
                : HealthCheckResult.Unhealthy($"OTel Collector status: {healthStatus?.Status ?? "unknown"}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Failed to check OTel Collector health status", ex);
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

    private record OtelCollectorHealthResponse(string Status, string UpSince, string Uptime);
}
