using System.Text.Json;
using System.Text.Json.Serialization;

namespace BuildingBlocks.AspireIntegrations.Zipkin;

using Microsoft.Extensions.Diagnostics.HealthChecks;

internal sealed class ZipkinHealthCheck(ZipkinResource resource) : IHealthCheck
{
    private readonly HttpClient _httpClient = new();
    private readonly Uri _healthEndpoint = new(
        $"http://{resource.PrimaryEndpoint.Host}:{resource.PrimaryEndpoint.Port}/health"
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
                    $"Zipkin health check failed with HTTP status: {response.StatusCode}"
                );
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var healthStatus = JsonSerializer.Deserialize<ZipkinHealthResponse>(
                content,
                CreateDefaultSerializerOptions()
            );

            if (healthStatus?.Status == "UP" && healthStatus.Zipkin.Status == "UP")
            {
                return HealthCheckResult.Healthy("Zipkin is healthy");
            }

            return HealthCheckResult.Unhealthy(
                $"Zipkin health check failed. Status: {healthStatus?.Status}, Zipkin Status: {healthStatus?.Zipkin?.Status}"
            );
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Failed to check Zipkin health status", ex);
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

    // Model for Zipkin's health response
    private record ZipkinHealthResponse(string Status, ZipkinComponentStatus Zipkin);

    private record ZipkinComponentStatus(string Status, Dictionary<string, object> Details);
}
