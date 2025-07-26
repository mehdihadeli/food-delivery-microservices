namespace BuildingBlocks.AspireIntegrations.Zipkin;

using System.Net.Sockets;
using Microsoft.Extensions.Diagnostics.HealthChecks;

internal sealed class ZipkinHealthCheck(ZipkinResource resource, string connectionString) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            // Get the Zipkin endpoint details
            var host = resource.PrimaryEndpoint.Host;
            var port = resource.PrimaryEndpoint.Port;

            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(host, port, cancellationToken);

            return HealthCheckResult.Healthy("Successfully connected to Zipkin endpoint.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Failed to connect to Zipkin endpoint.", ex);
        }
    }
}
