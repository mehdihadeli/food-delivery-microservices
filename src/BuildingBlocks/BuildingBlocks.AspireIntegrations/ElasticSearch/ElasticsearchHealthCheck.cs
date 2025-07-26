using System.Net.Sockets;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.AspireIntegrations.ElasticSearch;

internal sealed class ElasticsearchHealthCheck(ElasticsearchResource resource, string connectionString) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var host = resource.PrimaryEndpoint.Host;
            var port = resource.PrimaryEndpoint.Port;

            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(host, port, cancellationToken);

            return HealthCheckResult.Healthy("Successfully connected to Elasticsearch endpoint.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Failed to connect to Elasticsearch endpoint.", ex);
        }
    }
}
