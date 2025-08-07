using Aspire.Hosting.ApplicationModel;

namespace BuildingBlocks.AspireIntegrations.Jaeger;

public class JaegerResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    internal const string PrimaryEndpointName = "otlp-grpc";

    internal const string QueryHttpEndpointName = "http";
    internal const string QueryAdminHTTPEndpointName = "admin-http";
    internal const string AgentEndpointName = "agent";
    internal const string CollectorEndpointName = "collector";
    internal const string OtlpGrpcEndpointName = "otlp-grpc";
    internal const string OtlpHttpEndpointName = "otlp-http";

    public const string DefaultResourceName = "jaeger";
    public const string DataTargetFolder = "/var/lib/jaeger";

    // QueryHTTP is the default port for UI and Query API (e.g. /api/* endpoints)
    public const int QueryHttpContainerPort = 16686;
    public const int ProxyOrContainerHostQueryHttpPort = 16686;

    public const int AgentContainerPort = 6831;
    public const int ProxyOrContainerHostAgentPort = 6831;

    // CollectorHTTP is the default port for HTTP server for sending spans (e.g. /api/traces endpoint)
    public const int CollectorContainerPort = 14268;
    public const int ProxyOrContainerHostCollectorPort = 14268;

    public const int OtlpGrpcContainerPort = 4317;
    public const int ProxyOrContainerHostOtlpGrpcPort = 4317;

    public const int OtlpHttpContainerPort = 4318;
    public const int ProxyOrContainerHostOtlpHttpPort = 4318;

    private EndpointReference? _primaryEndpoint;
    private EndpointReference? _queryHttpEndpoint;
    private EndpointReference? _agentEndpoint;
    private EndpointReference? _collectorEndpoint;
    private EndpointReference? _otlpGrpcEndpoint;
    private EndpointReference? _otlpHttpEndpoint;

    public EndpointReference PrimaryEndpoint => _primaryEndpoint ??= new(this, PrimaryEndpointName);
    public EndpointReference QueryHttpEndpoint => _queryHttpEndpoint ??= new(this, QueryHttpEndpointName);
    public EndpointReference AgentEndpoint => _agentEndpoint ??= new(this, AgentEndpointName);
    public EndpointReference CollectorEndpoint => _collectorEndpoint ??= new(this, CollectorEndpointName);
    public EndpointReference OtlpGrpcEndpoint => _otlpGrpcEndpoint ??= new(this, OtlpGrpcEndpointName);
    public EndpointReference OtlpHttpEndpoint => _otlpHttpEndpoint ??= new(this, OtlpHttpEndpointName);

    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"http://{PrimaryEndpoint.Property(EndpointProperty.Host)}:{PrimaryEndpoint.Property(EndpointProperty.Port)}"
        );
}
