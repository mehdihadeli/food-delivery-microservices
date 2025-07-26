using System.Net.Sockets;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.AspireIntegrations.OpenTelemetryCollector;

internal sealed class OpenTelemetryCollectorHealthCheck(
    OpenTelemetryCollectorResource resource,
    string connectionString
) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            // Check the health endpoint
            var host = resource.PrimaryEndpoint.Host;
            var port = resource.PrimaryEndpoint.Port;

            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(host, port, cancellationToken);

            return HealthCheckResult.Healthy("Successfully connected to OTLP gRPC endpoint.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Failed to connect to OTLP gRPC endpoint.", ex);
        }
    }
}
