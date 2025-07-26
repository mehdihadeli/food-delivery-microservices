namespace BuildingBlocks.AspireIntegrations.Zipkin;

using Aspire.Hosting.ApplicationModel;

public class ZipkinResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    internal const string EndpointName = "http";
    public const string DefaultResourceName = "zipkin";
    public const string DataTargetFolder = "/zipkin";

    public const int ContainerPort = 9411;
    public const int ProxyOrContainerHostPort = 9411;

    private EndpointReference? _primaryEndpoint;

    public EndpointReference PrimaryEndpoint => _primaryEndpoint ??= new(this, EndpointName);

    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"http://{PrimaryEndpoint.Property(EndpointProperty.Host)}:{PrimaryEndpoint.Property(EndpointProperty.Port)}"
        );
}
