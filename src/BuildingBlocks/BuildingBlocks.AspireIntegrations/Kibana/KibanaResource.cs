using Aspire.Hosting.ApplicationModel;

namespace BuildingBlocks.AspireIntegrations.Kibana;

public class KibanaResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    internal const string PrimaryEndpointName = "http";
    internal const string HttpEndpointName = "http";

    public const string DefaultResourceName = "kibana";
    public const string DataTargetPath = "/usr/share/kibana/data";

    public const int ContainerPort = 5601;
    public const int ProxyOrContainerHostPort = 5601;

    private EndpointReference? _primaryEndpoint;
    private EndpointReference? _httpEndpoint;

    public EndpointReference PrimaryEndpoint => _primaryEndpoint ??= new(this, PrimaryEndpointName);
    public EndpointReference HttpEndpoint => _httpEndpoint ??= new(this, HttpEndpointName);

    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"http://{PrimaryEndpoint.Property(EndpointProperty.Host)}:{PrimaryEndpoint.Property(EndpointProperty.Port)}"
        );
}
