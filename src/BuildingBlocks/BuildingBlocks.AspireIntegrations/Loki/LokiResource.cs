using Aspire.Hosting.ApplicationModel;

namespace BuildingBlocks.AspireIntegrations.Loki;

public class LokiResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    internal const string PrimaryEndpointName = "http";
    internal const string HttpEndpointName = "http";
    internal const string GrpcEndpointName = "grpc";

    public const string DefaultResourceName = "loki";
    public const string DataTargetFolder = "/loki";
    public const string ConfigTargetPath = "/etc/loki/local-config.yaml";

    public const int HttpContainerPort = 3100;
    public const int ProxyOrContainerHostHttpPort = 3100;

    public const int GrpcContainerPort = 9096;
    public const int ProxyOrContainerHostGrpcPort = 9096;

    private EndpointReference? _primaryEndpoint;
    private EndpointReference? _httpEndpoint;
    private EndpointReference? _grpcEndpoint;

    public EndpointReference PrimaryEndpoint => _primaryEndpoint ??= new(this, PrimaryEndpointName);
    public EndpointReference HttpEndpoint => _httpEndpoint ??= new(this, HttpEndpointName);
    public EndpointReference GrpcEndpoint => _grpcEndpoint ??= new(this, GrpcEndpointName);

    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"http://{PrimaryEndpoint.Property(EndpointProperty.Host)}:{PrimaryEndpoint.Property(EndpointProperty.Port)}"
        );
}
