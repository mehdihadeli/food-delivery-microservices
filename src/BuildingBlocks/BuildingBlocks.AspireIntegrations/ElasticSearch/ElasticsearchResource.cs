using Aspire.Hosting.ApplicationModel;

namespace BuildingBlocks.AspireIntegrations.ElasticSearch;

public class ElasticsearchResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    internal const string PrimaryEndpointName = "http";
    internal const string HttpEndpointName = "http";
    internal const string TransportEndpointName = "transport";

    public const string DefaultResourceName = "elasticsearch";
    public const string DataTargetPath = "/usr/share/elasticsearch/data";

    public const int HttpContainerPort = 9200;
    public const int ProxyOrContainerHostHttpPort = 9200;

    public const int TransportContainerPort = 9300;
    public const int ProxyOrContainerHostTransportPort = 9300;

    private EndpointReference? _primaryEndpoint;
    private EndpointReference? _httpEndpoint;
    private EndpointReference? _transportEndpoint;

    public EndpointReference PrimaryEndpoint => _primaryEndpoint ??= new(this, PrimaryEndpointName);
    public EndpointReference HttpEndpoint => _httpEndpoint ??= new(this, HttpEndpointName);
    public EndpointReference TransportEndpoint => _transportEndpoint ??= new(this, TransportEndpointName);

    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"http://{PrimaryEndpoint.Property(EndpointProperty.Host)}:{PrimaryEndpoint.Property(EndpointProperty.Port)}"
        );
}
