using System.Diagnostics.CodeAnalysis;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.AspireIntegrations.AspireDashBoard;

[Experimental("ASPIREPROXYENDPOINTS001")]
public static class AspireDashboardBuilderExtensions
{
    /// <summary>
    /// Configures an Aspire Dashboard resource with flexible port binding options.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method supports four distinct operational modes based on proxy and port configuration:
    /// </para>
    ///
    /// <list type="table">
    ///   <listheader>
    ///     <term>Mode</term>
    ///     <term>proxyEnabled</term>
    ///     <term>Port Value</term>
    ///     <term>Behavior</term>
    ///   </listheader>
    ///   <item>
    ///     <term>Proxy with fixed port</term>
    ///     <term>true</term>
    ///     <term>Specified port</term>
    ///     <description>
    ///       Aspire generates random host ports for the container and uses the specified ports (typically 18888/18889) as proxy ports.
    ///       The proxy routes traffic from these fixed proxy ports to the container's random host ports.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term>Proxy with dynamic port</term>
    ///     <term>true</term>
    ///     <term>null</term>
    ///     <description>
    ///       Aspire generates both random host ports for the container and random proxy ports.
    ///       The proxy routes traffic from the random proxy ports to the container's random host ports.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term>Direct with fixed port</term>
    ///     <term>false</term>
    ///     <term>Specified port</term>
    ///     <description>
    ///       The container binds directly to the specified host ports (typically 18888/18889) with no proxy involved.
    ///       Traffic is routed directly to the container through these fixed host ports.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term>Direct with dynamic port</term>
    ///     <term>false</term>
    ///     <term>null</term>
    ///     <description>
    ///       The container binds to random available host ports with no proxy involved.
    ///       Traffic is routed directly to the container through these random host ports.
    ///     </description>
    ///   </item>
    /// </list>
    /// <para>
    /// Note: The Aspire Dashboard exposes two ports - Dashboard (18888) for the web UI and OTLP (18889) for telemetry ingestion.
    /// Each port follows the same proxy/host binding behavior described above.
    /// </para>
    /// </remarks>
    /// <param name="builder">The distributed application builder</param>
    /// <param name="nameOrConnectionStringName">Name of the resource or connection string key</param>
    /// <param name="useExistingServerInstance">
    /// When true and in publish mode, references an existing Dashboard instance instead of creating a container (default: false)
    /// </param>
    /// <param name="proxyEnabled">
    /// Enables Aspire's proxy for endpoint routing. When true, host ports are not exposed directly (default: true)
    /// </param>
    /// <param name="persistenceEnabled">
    /// Configures the container with persistent storage when true (default: false)
    /// </param>
    /// <param name="allowAnonymousAccess">
    /// When true, disables authentication requirements for the dashboard (default: true)
    /// </param>
    /// <param name="proxyOrContainerHostDashboardPort">
    /// Port for the Dashboard UI (18888). Behavior depends on proxyEnabled:
    /// - When proxyEnabled: Proxy port (null for auto-assigned)
    /// - When !proxyEnabled: Host port (null for auto-assigned)
    /// </param>
    /// <param name="proxyOrContainerHostOtlpPort">
    /// Port for OTLP telemetry ingestion (18889). Behavior depends on proxyEnabled:
    /// - When proxyEnabled: Proxy port (null for auto-assigned)
    /// - When !proxyEnabled: Host port (null for auto-assigned)
    /// </param>
    /// <returns>An IResourceBuilder for the configured Aspire Dashboard resource</returns>
    public static IResourceBuilder<IResourceWithConnectionString> AddAspireDashboard(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string nameOrConnectionStringName,
        bool useExistingServerInstance = false,
        bool proxyEnabled = true,
        bool persistenceEnabled = false,
        bool allowAnonymousAccess = true,
        int? proxyOrContainerHostDashboardPort = AspireDashboardResource.ProxyOrContainerHostDashboardPort,
        int? proxyOrContainerHostOtlpPort = AspireDashboardResource.ProxyOrContainerHostOtlpGrpcPort
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(nameOrConnectionStringName);

        if (useExistingServerInstance && builder.ExecutionContext.IsPublishMode)
        {
            return builder.AddConnectionString(nameOrConnectionStringName);
        }

        var dashboardResource = new AspireDashboardResource(nameOrConnectionStringName);

        string? connectionString = null;

        builder.Eventing.Subscribe<ConnectionStringAvailableEvent>(
            dashboardResource,
            async (@event, cancellationToken) =>
            {
                connectionString =
                    await dashboardResource
                        .ConnectionStringExpression.GetValueAsync(cancellationToken)
                        .ConfigureAwait(false)
                    ?? throw new DistributedApplicationException(
                        $"ConnectionStringAvailableEvent was published for the '{dashboardResource.Name}' resource but the connection string was null."
                    );
            }
        );

        var healthCheckKey = $"{nameOrConnectionStringName}_check";
        builder
            .Services.AddHealthChecks()
            .Add(
                new HealthCheckRegistration(
                    healthCheckKey,
                    _ => new AspireDashboardHealthCheck(dashboardResource, connectionString!),
                    failureStatus: default,
                    tags: default,
                    timeout: default
                )
            );

        var dashboard = builder
            .AddResource(dashboardResource)
            .WithImage(AspireDashboardContainerImageTags.Image, AspireDashboardContainerImageTags.Tag)
            .WithImageRegistry(AspireDashboardContainerImageTags.Registry)
            .WithContainerName(nameOrConnectionStringName)
            // Dashboard UI endpoint
            .WithEndpoint(
                port: proxyOrContainerHostDashboardPort,
                targetPort: AspireDashboardResource.HttpContainerPort,
                name: AspireDashboardResource.HttpEndpointName,
                isProxied: proxyEnabled,
                isExternal: true,
                scheme: "http"
            )
            // OTLP exporter endpoint
            .WithEndpoint(
                port: proxyOrContainerHostOtlpPort,
                targetPort: AspireDashboardResource.OtlpGrpcContainerPort,
                name: AspireDashboardResource.OtlpGrpcEndpointName,
                isProxied: proxyEnabled,
                scheme: "tcp",
                isExternal: false
            )
            .WithEnvironment(context => ConfigureDashboardContainer(context, allowAnonymousAccess))
            .WithHealthCheck(healthCheckKey);

        if (builder.ExecutionContext.IsPublishMode || persistenceEnabled)
        {
            dashboard.WithLifetime(ContainerLifetime.Persistent);
            dashboard.WithDataVolume("aspire_dashboard_data");
        }

        dashboard.WithEndpointProxySupport(proxyEnabled);

        return dashboard;
    }

    public static IResourceBuilder<AspireDashboardResource> WithDataVolume(
        this IResourceBuilder<AspireDashboardResource> builder,
        string? name = null
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.WithVolume(
            name ?? VolumeNameGenerator.Generate(builder, "data"),
            AspireDashboardResource.DataTargetFolder
        );
    }

    public static IResourceBuilder<AspireDashboardResource> WithDataBindMount(
        this IResourceBuilder<AspireDashboardResource> builder,
        string source
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(source);
        return builder.WithBindMount(source, AspireDashboardResource.DataTargetFolder);
    }

    private static void ConfigureDashboardContainer(EnvironmentCallbackContext context, bool allowAnonymous)
    {
        if (allowAnonymous)
        {
            context.EnvironmentVariables.Add("DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS", allowAnonymous.ToString());
        }
    }
}
