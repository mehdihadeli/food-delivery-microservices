using Aspire.Hosting.ApplicationModel;

namespace BuildingBlocks.AspireIntegrations.Jaeger;

public class JaegerResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    internal const string PrimaryEndpointName = "otlp-grpc";

    internal const string UiEndpointName = "ui";
    internal const string AgentEndpointName = "agent";
    internal const string CollectorEndpointName = "collector";
    internal const string OtlpGrpcEndpointName = "otlp-grpc";
    internal const string OtlpHttpEndpointName = "otlp-http";

    public const string DefaultResourceName = "jaeger";
    public const string DataTargetFolder = "/var/lib/jaeger";

    public const int UiContainerPort = 16686;
    public const int ProxyOrContainerHostUiPort = 16686;

    public const int AgentContainerPort = 6831;
    public const int ProxyOrContainerHostAgentPort = 6831;

    public const int CollectorContainerPort = 14268;
    public const int ProxyOrContainerHostCollectorPort = 14268;

    public const int OtlpGrpcContainerPort = 4317;
    public const int ProxyOrContainerHostOtlpGrpcPort = 4317;

    public const int OtlpHttpContainerPort = 4318;
    public const int ProxyOrContainerHostOtlpHttpPort = 4318;

    private EndpointReference? _primaryEndpoint;
    private EndpointReference? _uiEndpoint;
    private EndpointReference? _agentEndpoint;
    private EndpointReference? _collectorEndpoint;
    private EndpointReference? _otlpGrpcEndpoint;
    private EndpointReference? _otlpHttpEndpoint;

    public EndpointReference PrimaryEndpoint => _primaryEndpoint ??= new(this, PrimaryEndpointName);
    public EndpointReference UiEndpoint => _uiEndpoint ??= new(this, UiEndpointName);
    public EndpointReference AgentEndpoint => _agentEndpoint ??= new(this, AgentEndpointName);
    public EndpointReference CollectorEndpoint => _collectorEndpoint ??= new(this, CollectorEndpointName);
    public EndpointReference OtlpGrpcEndpoint => _otlpGrpcEndpoint ??= new(this, OtlpGrpcEndpointName);
    public EndpointReference OtlpHttpEndpoint => _otlpHttpEndpoint ??= new(this, OtlpHttpEndpointName);

    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"http://{PrimaryEndpoint.Property(EndpointProperty.Host)}:{PrimaryEndpoint.Property(EndpointProperty.Port)}"
        );
}
