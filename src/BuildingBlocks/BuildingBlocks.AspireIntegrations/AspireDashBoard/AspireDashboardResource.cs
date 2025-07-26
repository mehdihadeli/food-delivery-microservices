using Aspire.Hosting.ApplicationModel;

namespace BuildingBlocks.AspireIntegrations.AspireDashBoard;

public class AspireDashboardResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    internal const string PrimaryEndpointName = "http";

    internal const string HttpEndpointName = "http";
    internal const string OtlpGrpcEndpointName = "otlp-grpc";

    public const string DefaultResourceName = "aspire-dashboard";
    public const string DataTargetFolder = "/var/lib/aspire-dashboard";

    public const int HttpContainerPort = 18888;
    public const int ProxyOrContainerHostDashboardPort = 18888;

    public const int OtlpGrpcContainerPort = 18889;
    public const int ProxyOrContainerHostOtlpGrpcPort = 4319;

    private EndpointReference? _primaryEndpoint;
    private EndpointReference? _dashboardEndpoint;
    private EndpointReference? _otlpEndpoint;

    public EndpointReference PrimaryEndpoint => _primaryEndpoint ??= new(this, PrimaryEndpointName);
    public EndpointReference DashboardEndpoint => _dashboardEndpoint ??= new(this, HttpEndpointName);
    public EndpointReference OtlpGrpcEndpoint => _otlpEndpoint ??= new(this, OtlpGrpcEndpointName);

    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"http://{PrimaryEndpoint.Property(EndpointProperty.Host)}:{PrimaryEndpoint.Property(EndpointProperty.Port)}"
        );
}
