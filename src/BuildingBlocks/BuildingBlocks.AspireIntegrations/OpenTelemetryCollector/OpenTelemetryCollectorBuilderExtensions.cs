using System.Diagnostics.CodeAnalysis;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.AspireIntegrations.OpenTelemetryCollector;

[Experimental("ASPIREPROXYENDPOINTS001")]
public static class OpenTelemetryCollectorBuilderExtensions
{
    /// <summary>
    /// Configures an OpenTelemetry Collector as an Aspire resource with flexible port binding options.
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
    ///       Aspire generates random host ports for the container and uses the specified ports as proxy ports.
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
    ///       The container binds directly to the specified host ports with no proxy involved.
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
    /// Note: The Collector exposes multiple ports for different protocols and extensions.
    /// Each port follows the same proxy/host binding behavior described above.
    /// </para>
    /// </remarks>
    /// <param name="builder">The distributed application builder</param>
    /// <param name="nameOrConnectionStringName">Name of the resource or connection string key</param>
    /// <param name="useExistingServerInstance">
    /// When true and in publish mode, references an existing Collector instance instead of creating a container (default: false)
    /// </param>
    /// <param name="proxyEnabled">
    /// Enables Aspire's proxy for endpoint routing. When true, host ports are not exposed directly (default: true)
    /// </param>
    /// <param name="persistenceEnabled">
    /// Configures the container with persistent storage when true (default: false)
    /// </param>
    /// <param name="configBindMountPath">
    /// Optional path to a Collector configuration file to mount into the container
    /// </param>
    /// <param name="proxyOrContainerHostPprofPort">
    /// Port for the pprof extension (1888). Behavior depends on proxyEnabled:
    /// - When proxyEnabled: Proxy port (null for auto-assigned)
    /// - When !proxyEnabled: Host port (null for auto-assigned)
    /// </param>
    /// <param name="proxyOrContainerHostMetricsPort">
    /// Port for Prometheus metrics (8888). Behavior depends on proxyEnabled:
    /// - When proxyEnabled: Proxy port (null for auto-assigned)
    /// - When !proxyEnabled: Host port (null for auto-assigned)
    /// </param>
    /// <param name="proxyOrContainerHostExporterMetricsPort">
    /// Port for Prometheus exporter metrics (8889). Behavior depends on proxyEnabled:
    /// - When proxyEnabled: Proxy port (null for auto-assigned)
    /// - When !proxyEnabled: Host port (null for auto-assigned)
    /// </param>
    /// <param name="proxyOrContainerHostHealthCheckPort">
    /// Port for health check extension (13133). Behavior depends on proxyEnabled:
    /// - When proxyEnabled: Proxy port (null for auto-assigned)
    /// - When !proxyEnabled: Host port (null for auto-assigned)
    /// </param>
    /// <param name="proxyOrContainerHostOtlpGrpcPort">
    /// Port for OTLP gRPC receiver (4317). Behavior depends on proxyEnabled:
    /// - When proxyEnabled: Proxy port (null for auto-assigned)
    /// - When !proxyEnabled: Host port (null for auto-assigned)
    /// </param>
    /// <param name="proxyOrContainerHostOtlpHttpPort">
    /// Port for OTLP HTTP receiver (4318). Behavior depends on proxyEnabled:
    /// - When proxyEnabled: Proxy port (null for auto-assigned)
    /// - When !proxyEnabled: Host port (null for auto-assigned)
    /// </param>
    /// <param name="proxyOrContainerHostZPagesPort">
    /// Port for zPages extension (55679). Behavior depends on proxyEnabled:
    /// - When proxyEnabled: Proxy port (null for auto-assigned)
    /// - When !proxyEnabled: Host port (null for auto-assigned)
    /// </param>
    /// <param name="waitForDependencies">
    /// Optional array of resources that the Collector should wait for before starting
    /// </param>
    /// <returns>An IResourceBuilder for the configured OpenTelemetry Collector resource</returns>
    public static IResourceBuilder<IResourceWithConnectionString> AddAspireOpenTelemetryCollector(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string nameOrConnectionStringName,
        bool useExistingServerInstance = false,
        bool proxyEnabled = true,
        bool persistenceEnabled = false,
        string? configBindMountPath = null,
        int? proxyOrContainerHostPprofPort = OpenTelemetryCollectorResource.ProxyOrContainerHostPprofPort,
        int? proxyOrContainerHostMetricsPort = OpenTelemetryCollectorResource.ProxyOrContainerHostMetricsPort,
        int? proxyOrContainerHostExporterMetricsPort =
            OpenTelemetryCollectorResource.ProxyOrContainerHostExporterMetricsPort,
        int? proxyOrContainerHostHealthCheckPort = OpenTelemetryCollectorResource.ProxyOrContainerHostHealthCheckPort,
        int? proxyOrContainerHostOtlpGrpcPort = OpenTelemetryCollectorResource.ProxyOrContainerHostOtlpGrpcPort,
        int? proxyOrContainerHostOtlpHttpPort = OpenTelemetryCollectorResource.ProxyOrContainerHostOtlpHttpPort,
        int? proxyOrContainerHostZPagesPort = OpenTelemetryCollectorResource.ProxyOrContainerHostZPagesPort,
        IResourceBuilder<IResourceWithConnectionString>[]? waitForDependencies = null
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(nameOrConnectionStringName);

        if (useExistingServerInstance && builder.ExecutionContext.IsPublishMode)
        {
            return builder.AddConnectionString(nameOrConnectionStringName);
        }

        var otelResource = new OpenTelemetryCollectorResource(nameOrConnectionStringName);

        builder.Eventing.Subscribe<ConnectionStringAvailableEvent>(
            otelResource,
            async (@event, cancellationToken) =>
            {
                var connectionString =
                    await otelResource.ConnectionStringExpression.GetValueAsync(cancellationToken).ConfigureAwait(false)
                    ?? throw new DistributedApplicationException(
                        $"ConnectionStringAvailableEvent was published for the '{otelResource.Name}' resource but the connection string was null."
                    );
            }
        );

        var healthCheckKey = $"{nameOrConnectionStringName}_check";
        builder
            .Services.AddHealthChecks()
            .Add(
                new HealthCheckRegistration(
                    healthCheckKey,
                    _ => new OpenTelemetryCollectorHealthCheck(otelResource),
                    failureStatus: default,
                    tags: default,
                    timeout: default
                )
            );

        var otel = builder
            .AddResource(otelResource)
            .WithImage(OpenTelemetryCollectorContainerImageTags.Image, OpenTelemetryCollectorContainerImageTags.Tag)
            .WithImageRegistry(OpenTelemetryCollectorContainerImageTags.Registry)
            .WithContainerName(nameOrConnectionStringName)
            .WithEndpoint(
                port: proxyOrContainerHostOtlpGrpcPort,
                targetPort: OpenTelemetryCollectorResource.OtlpGrpcContainerPort,
                name: OpenTelemetryCollectorResource.OtlpGrpcEndpointName,
                isProxied: proxyEnabled,
                isExternal: true,
                scheme: "tcp"
            )
            .WithEndpoint(
                port: proxyOrContainerHostOtlpHttpPort,
                targetPort: OpenTelemetryCollectorResource.OtlpHttpContainerPort,
                name: OpenTelemetryCollectorResource.OtlpHttpEndpointName,
                isProxied: proxyEnabled,
                isExternal: true
            )
            .WithEndpoint(
                port: proxyOrContainerHostPprofPort,
                targetPort: OpenTelemetryCollectorResource.PprofContainerPort,
                name: OpenTelemetryCollectorResource.PprofEndpointName,
                isProxied: proxyEnabled,
                scheme: "http",
                isExternal: true
            )
            .WithEndpoint(
                port: proxyOrContainerHostMetricsPort,
                targetPort: OpenTelemetryCollectorResource.MetricsContainerPort,
                name: OpenTelemetryCollectorResource.MetricsEndpointName,
                isProxied: proxyEnabled,
                scheme: "http",
                isExternal: true
            )
            .WithEndpoint(
                port: proxyOrContainerHostExporterMetricsPort,
                targetPort: OpenTelemetryCollectorResource.ExporterMetricsContainerPort,
                name: OpenTelemetryCollectorResource.ExporterMetricsEndpointName,
                isProxied: proxyEnabled,
                scheme: "http",
                isExternal: true
            )
            .WithEndpoint(
                port: proxyOrContainerHostHealthCheckPort,
                targetPort: OpenTelemetryCollectorResource.HealthCheckContainerPort,
                name: OpenTelemetryCollectorResource.HealthCheckEndpointName,
                isProxied: proxyEnabled,
                scheme: "http",
                isExternal: true
            )
            .WithEndpoint(
                port: proxyOrContainerHostZPagesPort,
                targetPort: OpenTelemetryCollectorResource.ZPagesContainerPort,
                name: OpenTelemetryCollectorResource.ZPagesEndpointName,
                isProxied: proxyEnabled,
                scheme: "http",
                isExternal: true
            )
            .WithEnvironment(ConfigureOtelCollectorContainer)
            .WithHealthCheck(healthCheckKey);

        if (!string.IsNullOrEmpty(configBindMountPath))
        {
            otel.WithConfigBindMount(configBindMountPath);
        }

        if (builder.ExecutionContext.IsPublishMode || persistenceEnabled)
        {
            otel.WithLifetime(ContainerLifetime.Persistent);
            otel.WithDataVolume("otel_data");
        }

        waitForDependencies?.ToList().ForEach(x => otel.WaitFor(x));

        otel.WithEndpointProxySupport(proxyEnabled);

        return otel;
    }

    public static IResourceBuilder<OpenTelemetryCollectorResource> WithConfigBindMount(
        this IResourceBuilder<OpenTelemetryCollectorResource> builder,
        string source
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(source);
        return builder
            .WithBindMount(source, OpenTelemetryCollectorResource.ConfigTargetPath)
            .WithArgs($"--config={OpenTelemetryCollectorResource.ConfigTargetPath}");
    }

    public static IResourceBuilder<OpenTelemetryCollectorResource> WithDataVolume(
        this IResourceBuilder<OpenTelemetryCollectorResource> builder,
        string? name = null
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.WithVolume(
            name ?? VolumeNameGenerator.Generate(builder, "data"),
            OpenTelemetryCollectorResource.DataTargetFolder
        );
    }

    public static IResourceBuilder<OpenTelemetryCollectorResource> WithDataBindMount(
        this IResourceBuilder<OpenTelemetryCollectorResource> builder,
        string source
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(source);
        return builder.WithBindMount(source, OpenTelemetryCollectorResource.DataTargetFolder);
    }

    private static void ConfigureOtelCollectorContainer(EnvironmentCallbackContext context)
    {
        // Default environment variables
        context.EnvironmentVariables.Add("SPLUNK_DEBUG_CONFIG_SERVER", "false");
    }
}
