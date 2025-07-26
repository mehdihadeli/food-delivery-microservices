using System.Net.Sockets;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.AspireIntegrations.Jaeger;

internal sealed class JaegerHealthCheck(JaegerResource resource, string connectionString) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            // Get Jaeger's gRPC endpoint details
            var host = resource.PrimaryEndpoint.Host;
            var port = resource.PrimaryEndpoint.Port;

            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(host, port, cancellationToken);

            return HealthCheckResult.Healthy("Successfully connected to Jaeger OTLP gRPC endpoint.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Failed to connect to Jaeger OTLP gRPC endpoint.", ex);
        }
    }
}
