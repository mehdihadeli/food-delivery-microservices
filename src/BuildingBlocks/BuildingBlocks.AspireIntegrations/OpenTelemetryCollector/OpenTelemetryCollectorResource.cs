using Aspire.Hosting.ApplicationModel;

namespace BuildingBlocks.AspireIntegrations.OpenTelemetryCollector;

public class OpenTelemetryCollectorResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    internal const string PrimaryEndpointName = "otlp-grpc";

    internal const string OtlpGrpcEndpointName = "otlp-grpc";
    internal const string OtlpHttpEndpointName = "otlp-http";

    internal const string PprofEndpointName = "pprof";
    internal const string MetricsEndpointName = "metrics";
    internal const string ExporterMetricsEndpointName = "exporter-metrics";
    internal const string HealthCheckEndpointName = "health";
    internal const string ZPagesEndpointName = "zpages";

    public const string DefaultResourceName = "otel-collector";
    public const string ConfigTargetPath = "/etc/otelcol-contrib/config.yaml";
    public const string DataTargetFolder = "/var/lib/otelcol";

    public const int OtlpGrpcContainerPort = 4317;
    public const int ProxyOrContainerHostOtlpGrpcPort = 4317;

    public const int OtlpHttpContainerPort = 4318;
    public const int ProxyOrContainerHostOtlpHttpPort = 4318;

    public const int PprofContainerPort = 1888;
    public const int ProxyOrContainerHostPprofPort = 11888;

    public const int MetricsContainerPort = 8888;
    public const int ProxyOrContainerHostMetricsPort = 8888;

    public const int ExporterMetricsContainerPort = 8889;
    public const int ProxyOrContainerHostExporterMetricsPort = 8889;

    public const int HealthCheckContainerPort = 13133;
    public const int ProxyOrContainerHostHealthCheckPort = 13133;

    public const int ZPagesContainerPort = 55679;
    public const int ProxyOrContainerHostZPagesPort = 55679;

    private EndpointReference? _primaryEndpoint;
    private EndpointReference? _otlpGrpcEndpoint;
    private EndpointReference? _otlpHttpEndpoint;
    private EndpointReference? _pprofEndpoint;
    private EndpointReference? _metricsEndpoint;
    private EndpointReference? _exporterMetricsEndpoint;
    private EndpointReference? _healthCheckEndpoint;
    private EndpointReference? _zPagesEndpoint;

    public EndpointReference PrimaryEndpoint => _primaryEndpoint ??= new(this, PrimaryEndpointName);
    public EndpointReference OtlpGrpcEndpoint => _otlpGrpcEndpoint ??= new(this, OtlpGrpcEndpointName);
    public EndpointReference OtlpHttpEndpoint => _otlpHttpEndpoint ??= new(this, OtlpHttpEndpointName);
    public EndpointReference PprofEndpoint => _pprofEndpoint ??= new(this, PprofEndpointName);
    public EndpointReference MetricsEndpoint => _metricsEndpoint ??= new(this, MetricsEndpointName);
    public EndpointReference ExporterMetricsEndpoint =>
        _exporterMetricsEndpoint ??= new(this, ExporterMetricsEndpointName);
    public EndpointReference HealthCheckEndpoint => _healthCheckEndpoint ??= new(this, HealthCheckEndpointName);
    public EndpointReference ZPagesEndpoint => _zPagesEndpoint ??= new(this, ZPagesEndpointName);

    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"http://{PrimaryEndpoint.Property(EndpointProperty.Host)}:{PrimaryEndpoint.Property(EndpointProperty.Port)}"
        );
}
