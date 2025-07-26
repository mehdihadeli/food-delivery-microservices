using Aspire.Hosting.ApplicationModel;

namespace BuildingBlocks.AspireIntegrations.Tempo;

public class TempoResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    internal const string PrimaryEndpointName = "otlp-grpc";

    internal const string HttpEndpointName = "http";
    internal const string GrpcEndpointName = "grpc";
    internal const string OtlpGrpcEndpointName = "otlp-grpc";
    internal const string OtlpHttpEndpointName = "otlp-http";

    public const string DefaultResourceName = "tempo";
    public const string DataTargetFolder = "/tempo";
    public const string ConfigTargetPath = "/etc/tempo.yaml";

    public const int HttpContainerPort = 3200;
    public const int ProxyOrContainerHostHttpPort = 3200;

    public const int GrpcContainerPort = 9095;
    public const int ProxyOrContainerHostGrpcPort = 9095;

    public const int OtlpGrpcContainerPort = 4317;
    public const int ProxyOrContainerHostOtlpGrpcPort = 4317;

    public const int OtlpHttpContainerPort = 4318;
    public const int ProxyOrContainerHostOtlpHttpPort = 4318;

    private EndpointReference? _primaryEndpoint;
    private EndpointReference? _httpEndpoint;
    private EndpointReference? _otlpHttpEndpoint;
    private EndpointReference? _otlpGrpcEndpoint;
    private EndpointReference? _grpcEndpoint;

    public EndpointReference PrimaryEndpoint => _primaryEndpoint ??= new(this, PrimaryEndpointName);
    public EndpointReference HttpEndpoint => _httpEndpoint ??= new(this, HttpEndpointName);
    public EndpointReference OtlpHttpEndpoint => _otlpHttpEndpoint ??= new(this, OtlpHttpEndpointName);
    public EndpointReference OtlpGrpcEndpoint => _otlpGrpcEndpoint ??= new(this, OtlpGrpcEndpointName);
    public EndpointReference GrpcEndpoint => _grpcEndpoint ??= new(this, GrpcEndpointName);

    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"http://{PrimaryEndpoint.Property(EndpointProperty.Host)}:{PrimaryEndpoint.Property(EndpointProperty.Port)}"
        );
}
