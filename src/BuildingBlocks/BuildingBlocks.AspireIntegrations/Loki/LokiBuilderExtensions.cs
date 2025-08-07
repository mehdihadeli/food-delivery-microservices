using System.Diagnostics.CodeAnalysis;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.AspireIntegrations.Loki;

[Experimental("ASPIREPROXYENDPOINTS001")]
public static class LokiBuilderExtensions
{
    /// <summary>
    /// Configures a Loki log aggregation backend as an Aspire resource with flexible port binding options.
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
    ///       Aspire generates random host ports for the container and uses the specified ports (typically 3100/9096) as proxy ports.
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
    ///       The container binds directly to the specified host ports (typically 3100/9096) with no proxy involved.
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
    /// </remarks>
    /// <param name="builder">The distributed application builder</param>
    /// <param name="nameOrConnectionStringName">Name of the resource or connection string key</param>
    /// <param name="useExistingServerInstance">
    /// When true and in publish mode, references an existing Loki instance instead of creating a container (default: false)
    /// </param>
    /// <param name="proxyEnabled">
    /// Enables Aspire's proxy for endpoint routing. When true, host ports are not exposed directly (default: true)
    /// </param>
    /// <param name="persistenceEnabled">
    /// Configures the container with persistent storage when true (default: false)
    /// </param>
    /// <param name="configBindMountPath">
    /// Optional path to a Loki configuration file to mount into the container
    /// </param>
    /// <param name="proxyOrContainerHostHttpPort">
    /// Port for the HTTP endpoint (3100). Behavior depends on proxyEnabled:
    /// - When proxyEnabled: Proxy port (null for auto-assigned)
    /// - When !proxyEnabled: Host port (null for auto-assigned)
    /// </param>
    /// <param name="proxyOrContainerHostGrpcPort">
    /// Port for the gRPC endpoint (9096). Behavior depends on proxyEnabled:
    /// - When proxyEnabled: Proxy port (null for auto-assigned)
    /// - When !proxyEnabled: Host port (null for auto-assigned)
    /// </param>
    /// <returns>An IResourceBuilder for the configured Loki resource</returns>
    public static IResourceBuilder<IResourceWithConnectionString> AddAspireLoki(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string nameOrConnectionStringName,
        bool useExistingServerInstance = false,
        bool proxyEnabled = true,
        bool persistenceEnabled = false,
        string? configBindMountPath = null,
        int? proxyOrContainerHostHttpPort = LokiResource.ProxyOrContainerHostHttpPort,
        int? proxyOrContainerHostGrpcPort = LokiResource.ProxyOrContainerHostGrpcPort
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(nameOrConnectionStringName);

        if (useExistingServerInstance && builder.ExecutionContext.IsPublishMode)
        {
            return builder.AddConnectionString(nameOrConnectionStringName);
        }

        var lokiResource = new LokiResource(nameOrConnectionStringName);

        builder.Eventing.Subscribe<ConnectionStringAvailableEvent>(
            lokiResource,
            async (@event, cancellationToken) =>
            {
                var connectionString =
                    await lokiResource.ConnectionStringExpression.GetValueAsync(cancellationToken).ConfigureAwait(false)
                    ?? throw new DistributedApplicationException(
                        $"ConnectionStringAvailableEvent was published for the '{lokiResource.Name}' resource but the connection string was null."
                    );
            }
        );

        var healthCheckKey = $"{nameOrConnectionStringName}_check";
        builder
            .Services.AddHealthChecks()
            .Add(
                new HealthCheckRegistration(
                    healthCheckKey,
                    _ => new LokiHealthCheck(lokiResource),
                    failureStatus: default,
                    tags: default,
                    timeout: default
                )
            );

        var loki = builder
            .AddResource(lokiResource)
            .WithImage(LokiContainerImageTags.Image, LokiContainerImageTags.Tag)
            .WithImageRegistry(LokiContainerImageTags.Registry)
            .WithContainerName(nameOrConnectionStringName)
            .WithHttpEndpoint(
                port: proxyOrContainerHostHttpPort,
                targetPort: LokiResource.HttpContainerPort,
                name: LokiResource.HttpEndpointName,
                isProxied: proxyEnabled
            )
            .WithEndpoint(
                port: proxyOrContainerHostGrpcPort,
                targetPort: LokiResource.GrpcContainerPort,
                name: LokiResource.GrpcEndpointName,
                isProxied: proxyEnabled,
                isExternal: false,
                scheme: "tcp"
            )
            .WithHealthCheck(healthCheckKey);

        if (!string.IsNullOrEmpty(configBindMountPath))
        {
            loki.WithConfigBindMount(configBindMountPath);
        }

        if (builder.ExecutionContext.IsPublishMode || persistenceEnabled)
        {
            loki.WithLifetime(ContainerLifetime.Persistent);
            loki.WithDataVolume("loki_data");
        }

        // // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/networking-overview#ports-and-proxies
        // // https://github.com/dotnet/aspire/issues/1637
        // // - aspire has a proxy for endpoints for 2 Reason:
        // // 1. to avoid forwarding the connection until the underling endpoint is ready
        // // 2. to route to multiple replicas of the underlying service
        // //
        // // - if proxy is disabled, this port will be the host port of the container, and if proxy is disabled by `WithEndpointProxySupport(false)`
        // // this port will use by aspire proxy to route to a random generated host container port.
        loki.WithEndpointProxySupport(proxyEnabled);

        return loki;
    }

    public static IResourceBuilder<LokiResource> WithDataVolume(
        this IResourceBuilder<LokiResource> builder,
        string? name = null
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.WithVolume(name ?? VolumeNameGenerator.Generate(builder, "data"), LokiResource.DataTargetFolder);
    }

    public static IResourceBuilder<LokiResource> WithDataBindMount(
        this IResourceBuilder<LokiResource> builder,
        string source
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(source);
        return builder.WithBindMount(source, LokiResource.DataTargetFolder);
    }

    public static IResourceBuilder<LokiResource> WithConfigBindMount(
        this IResourceBuilder<LokiResource> builder,
        string source
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(source);
        return builder
            .WithBindMount(source, LokiResource.ConfigTargetPath)
            .WithArgs($"-config.file={LokiResource.ConfigTargetPath}");
    }
}
