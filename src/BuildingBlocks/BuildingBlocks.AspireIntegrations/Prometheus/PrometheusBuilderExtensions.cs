using System.Diagnostics.CodeAnalysis;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.AspireIntegrations.Prometheus;

[Experimental("ASPIREPROXYENDPOINTS001")]
public static class PrometheusBuilderExtensions
{
    /// <summary>
    /// Configures a Prometheus monitoring backend as an Aspire resource with flexible port binding options.
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
    ///       Aspire generates a random host port for the container and uses the specified port (typically 9090) as the proxy port.
    ///       The proxy routes traffic from this fixed proxy port to the container's random host port.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term>Proxy with dynamic port</term>
    ///     <term>true</term>
    ///     <term>null</term>
    ///     <description>
    ///       Aspire generates both a random host port for the container and a random proxy port.
    ///       The proxy routes traffic from the random proxy port to the container's random host port.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term>Direct with fixed port</term>
    ///     <term>false</term>
    ///     <term>Specified port</term>
    ///     <description>
    ///       The container binds directly to the specified host port (typically 9090) with no proxy involved.
    ///       Traffic is routed directly to the container through this fixed host port.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term>Direct with dynamic port</term>
    ///     <term>false</term>
    ///     <term>null</term>
    ///     <description>
    ///       The container binds to a random available host port with no proxy involved.
    ///       Traffic is routed directly to the container through this random host port.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    /// <param name="builder">The distributed application builder</param>
    /// <param name="nameOrConnectionStringName">Name of the resource or connection string key</param>
    /// <param name="useExistingServerInstance">
    /// When true and in publish mode, references an existing Prometheus instance instead of creating a container (default: false)
    /// </param>
    /// <param name="proxyEnabled">
    /// Enables Aspire's proxy for endpoint routing. When true, host ports are not exposed directly (default: true)
    /// </param>
    /// <param name="persistenceEnabled">
    /// Configures the container with persistent storage when true (default: false)
    /// </param>
    /// <param name="configBindMountPath">
    /// Optional path to a Prometheus configuration file (prometheus.yml) to mount into the container
    /// </param>
    /// <param name="proxyOrContainerHostWebPort">
    /// Port for the web endpoint (9090). Behavior depends on proxyEnabled:
    /// - When proxyEnabled: Proxy port (null for auto-assigned)
    /// - When !proxyEnabled: Host port (null for auto-assigned)
    /// </param>
    /// <param name="enableRemoteWriteReceiver">
    /// Enables the remote write receiver endpoint when true (default: true)
    /// </param>
    /// <returns>An IResourceBuilder for the configured Prometheus resource</returns>
    public static IResourceBuilder<IResourceWithConnectionString> AddAspirePrometheus(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string nameOrConnectionStringName,
        bool useExistingServerInstance = false,
        bool proxyEnabled = true,
        bool persistenceEnabled = false,
        string? configBindMountPath = null,
        int? proxyOrContainerHostWebPort = PrometheusResource.ProxyOrContainerHostWebPort,
        bool enableRemoteWriteReceiver = true
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(nameOrConnectionStringName);

        if (useExistingServerInstance && builder.ExecutionContext.IsPublishMode)
        {
            return builder.AddConnectionString(nameOrConnectionStringName);
        }

        var prometheusResource = new PrometheusResource(nameOrConnectionStringName);

        builder.Eventing.Subscribe<ConnectionStringAvailableEvent>(
            prometheusResource,
            async (@event, cancellationToken) =>
            {
                var connectionString =
                    await prometheusResource
                        .ConnectionStringExpression.GetValueAsync(cancellationToken)
                        .ConfigureAwait(false)
                    ?? throw new DistributedApplicationException(
                        $"ConnectionStringAvailableEvent was published for the '{prometheusResource.Name}' resource but the connection string was null."
                    );
            }
        );

        var healthCheckKey = $"{nameOrConnectionStringName}_check";
        builder
            .Services.AddHealthChecks()
            .Add(
                new HealthCheckRegistration(
                    healthCheckKey,
                    _ => new PrometheusHealthCheck(prometheusResource),
                    failureStatus: default,
                    tags: default,
                    timeout: default
                )
            );

        var prometheus = builder
            .AddResource(prometheusResource)
            .WithImage(PrometheusContainerImageTags.Image, PrometheusContainerImageTags.Tag)
            .WithImageRegistry(PrometheusContainerImageTags.Registry)
            .WithContainerName(nameOrConnectionStringName)
            .WithEndpoint(
                port: proxyOrContainerHostWebPort,
                targetPort: PrometheusResource.WebContainerPort,
                name: PrometheusResource.HttpEndpointName,
                isProxied: proxyEnabled,
                scheme: "http",
                isExternal: true
            )
            .WithArgs(
                $"--storage.tsdb.path={PrometheusResource.DataTargetFolder}",
                "--web.console.libraries=/usr/share/prometheus/console_libraries",
                "--web.console.templates=/usr/share/prometheus/consoles"
            )
            .WithHealthCheck(healthCheckKey);

        if (enableRemoteWriteReceiver)
        {
            prometheus.WithArgs("--web.enable-remote-write-receiver");
        }

        if (!string.IsNullOrEmpty(configBindMountPath))
        {
            prometheus.WithConfigBindMount(configBindMountPath);
        }

        if (builder.ExecutionContext.IsPublishMode || persistenceEnabled)
        {
            prometheus.WithLifetime(ContainerLifetime.Persistent);
            prometheus.WithDataVolume("prometheus_data");
        }

        prometheus.WithEndpointProxySupport(proxyEnabled);

        return prometheus;
    }

    public static IResourceBuilder<PrometheusResource> WithDataVolume(
        this IResourceBuilder<PrometheusResource> builder,
        string? name = null
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.WithVolume(
            name ?? VolumeNameGenerator.Generate(builder, "data"),
            PrometheusResource.DataTargetFolder
        );
    }

    public static IResourceBuilder<PrometheusResource> WithDataBindMount(
        this IResourceBuilder<PrometheusResource> builder,
        string source
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(source);
        return builder.WithBindMount(source, PrometheusResource.DataTargetFolder);
    }

    /// <summary>
    /// set a path to the Prometheus configuration file to bind mount into the container like `./../configs/prometheus.yaml` which binds to a container path `/etc/prometheus/prometheus.yml`
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="sourceFile"></param>
    /// <returns></returns>
    public static IResourceBuilder<PrometheusResource> WithConfigBindMount(
        this IResourceBuilder<PrometheusResource> builder,
        string sourceFile
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(sourceFile);
        return builder
            .WithBindMount(sourceFile, PrometheusResource.ConfigTargetPath)
            .WithArgs($"--config.file={PrometheusResource.ConfigTargetPath}");
    }
}
