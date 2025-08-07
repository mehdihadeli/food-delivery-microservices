using Aspire.Hosting.ApplicationModel;

namespace BuildingBlocks.AspireIntegrations.Prometheus;

public class PrometheusResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    internal const string PrimaryEndpointName = "http";
    internal const string HttpEndpointName = "http";

    public const string DefaultResourceName = "prometheus";
    public const string ConfigTargetPath = "/etc/prometheus/prometheus.yml";
    public const string DataTargetFolder = "/prometheus";

    public const int WebContainerPort = 9090;
    public const int ProxyOrContainerHostWebPort = 9090;

    private EndpointReference? _primaryEndpoint;
    private EndpointReference? _httpEndpoint;

    public EndpointReference PrimaryEndpoint => _primaryEndpoint ??= new(this, PrimaryEndpointName);
    public EndpointReference HttpEndpoint => _httpEndpoint ??= new(this, HttpEndpointName);

    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"http://{PrimaryEndpoint.Property(EndpointProperty.Host)}:{PrimaryEndpoint.Property(EndpointProperty.Port)}"
        );
}
