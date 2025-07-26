using Aspire.Hosting.ApplicationModel;

namespace BuildingBlocks.AspireIntegrations.Grafana;

public class GrafanaResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    internal const string PrimaryEndpointName = "http";
    internal const string HttpEndpointName = "http";

    public const string DefaultResourceName = "grafana";
    public const string DataTargetFolder = "/var/lib/grafana";

    public const int ContainerPort = 3000;
    public const int ProxyOrContainerHostPort = 3000;

    public const string ProvisioningTargetPath = "/etc/grafana/provisioning";
    public const string DashboardsTargetPath = "/var/lib/grafana/dashboards";
    public const string ConfigTargetPath = "/etc/grafana/grafana.ini";

    private EndpointReference? _primaryEndpoint;
    private EndpointReference? _httpEndpoint;

    public EndpointReference PrimaryEndpoint => _primaryEndpoint ??= new(this, PrimaryEndpointName);
    public EndpointReference HttpEndpoint => _httpEndpoint ??= new(this, HttpEndpointName);

    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"http://{PrimaryEndpoint.Property(EndpointProperty.Host)}:{PrimaryEndpoint.Property(EndpointProperty.Port)}"
        );
}
