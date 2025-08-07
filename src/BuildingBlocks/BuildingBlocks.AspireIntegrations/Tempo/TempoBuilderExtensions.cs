using System.Diagnostics.CodeAnalysis;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.AspireIntegrations.Tempo;

// 1. If `proxyEnabled = true` and we set a port in `WithEndpoint`, then aspire generate a random host port for the container and since we are using a proxy and have set a port in `WithEndpoint`, that port will be used for the proxy port. This proxy port will be used for accessing the container created random host port.
// 2. If `proxyEnabled = true` and we set the port to `null` in `WithEndpoint`, then aspire generate a random host port for the container and since we are using a proxy and have set the port to `null` in `WithEndpoint`, a random proxy port will be generated for proxy port and this proxy port will be used for accessing the container generated random port.
// 3. If `proxyEnabled = false` and we set a port in `WithEndpoint`, then because `proxyEnabled=false`, there will be no proxy port. Since we set a port in `WithEndpoint`, that port will be used as the container's host port. This host port will be used for accessing the container.
// 4. If `proxyEnabled = false` and we set the port to `null` in `WithEndpoint`, then because `proxyEnabled=false`, there will be no proxy port. Since we set the port to `null` in `WithEndpoint`, a random host port will be generated. This host port will be used for accessing the container.

// https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/networking-overview

[Experimental("ASPIREPROXYENDPOINTS001")]
public static class TempoBuilderExtensions
{
    /// <summary>
    /// Configures a Tempo distributed tracing backend as an Aspire resource with flexible port binding options.
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
    ///       Aspire generates random host ports for the container and uses the specified ports (typically 3200/9095/4317/4318) as proxy ports.
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
    ///       The container binds directly to the specified host ports (typically 3200/9095/4317/4318) with no proxy involved.
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
    /// Note: Tempo exposes multiple ports for different protocols (HTTP, gRPC, OTLP).
    /// Each port follows the same proxy/host binding behavior described above.
    /// </para>
    /// </remarks>
    /// <param name="builder">The distributed application builder</param>
    /// <param name="nameOrConnectionStringName">Name of the resource or connection string key</param>
    /// <param name="useExistingServerInstance">
    /// When true and in publish mode, references an existing Tempo instance instead of creating a container (default: false)
    /// </param>
    /// <param name="proxyEnabled">
    /// Enables Aspire's proxy for endpoint routing. When true, host ports are not exposed directly (default: true)
    /// </param>
    /// <param name="persistenceEnabled">
    /// Configures the container with persistent storage when true (default: false)
    /// </param>
    /// <param name="configBindMountPath">
    /// Optional path to a Tempo configuration file (tempo.yaml) to mount into the container
    /// </param>
    /// <param name="proxyOrContainerHostHttpPort">
    /// Port for the HTTP endpoint (3200). Behavior depends on proxyEnabled:
    /// - When proxyEnabled: Proxy port (null for auto-assigned)
    /// - When !proxyEnabled: Host port (null for auto-assigned)
    /// </param>
    /// <param name="proxyOrContainerHostGrpcPort">
    /// Port for the gRPC endpoint (9095). Behavior depends on proxyEnabled:
    /// - When proxyEnabled: Proxy port (null for auto-assigned)
    /// - When !proxyEnabled: Host port (null for auto-assigned)
    /// </param>
    /// <param name="proxyOrContainerHostOtlpGrpcPort">
    /// Port for the OTLP gRPC endpoint (4317). Behavior depends on proxyEnabled:
    /// - When proxyEnabled: Proxy port (null for auto-assigned)
    /// - When !proxyEnabled: Host port (null for auto-assigned)
    /// </param>
    /// <param name="proxyOrContainerHostOtlpHttpPort">
    /// Port for the OTLP HTTP endpoint (4318). Behavior depends on proxyEnabled:
    /// - When proxyEnabled: Proxy port (null for auto-assigned)
    /// - When !proxyEnabled: Host port (null for auto-assigned)
    /// </param>
    /// <returns>An IResourceBuilder for the configured Tempo resource</returns>
    public static IResourceBuilder<IResourceWithConnectionString> AddAspireTempo(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string nameOrConnectionStringName,
        bool useExistingServerInstance = false,
        bool proxyEnabled = true,
        bool persistenceEnabled = false,
        string? configBindMountPath = null,
        int? proxyOrContainerHostHttpPort = TempoResource.ProxyOrContainerHostHttpPort,
        int? proxyOrContainerHostGrpcPort = TempoResource.ProxyOrContainerHostGrpcPort,
        int? proxyOrContainerHostOtlpGrpcPort = TempoResource.ProxyOrContainerHostOtlpGrpcPort,
        int? proxyOrContainerHostOtlpHttpPort = TempoResource.ProxyOrContainerHostOtlpHttpPort
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(nameOrConnectionStringName);

        if (useExistingServerInstance && builder.ExecutionContext.IsPublishMode)
        {
            return builder.AddConnectionString(nameOrConnectionStringName);
        }

        var tempoResource = new TempoResource(nameOrConnectionStringName);

        builder.Eventing.Subscribe<ConnectionStringAvailableEvent>(
            tempoResource,
            async (@event, cancellationToken) =>
            {
                var connectionString =
                    await tempoResource
                        .ConnectionStringExpression.GetValueAsync(cancellationToken)
                        .ConfigureAwait(false)
                    ?? throw new DistributedApplicationException(
                        $"ConnectionStringAvailableEvent was published for the '{tempoResource.Name}' resource but the connection string was null."
                    );
            }
        );

        var healthCheckKey = $"{nameOrConnectionStringName}_check";
        builder
            .Services.AddHealthChecks()
            .Add(
                new HealthCheckRegistration(
                    healthCheckKey,
                    _ => new TempoHealthCheck(tempoResource),
                    failureStatus: default,
                    tags: default,
                    timeout: default
                )
            );

        var tempo = builder
            .AddResource(tempoResource)
            .WithImage(TempoContainerImageTags.Image, TempoContainerImageTags.Tag)
            .WithImageRegistry(TempoContainerImageTags.Registry)
            .WithContainerName(nameOrConnectionStringName)
            .WithEndpoint(
                port: proxyOrContainerHostHttpPort,
                targetPort: TempoResource.HttpContainerPort,
                name: TempoResource.HttpEndpointName,
                isProxied: proxyEnabled,
                isExternal: false,
                scheme: "http"
            )
            .WithEndpoint(
                port: proxyOrContainerHostGrpcPort,
                targetPort: TempoResource.GrpcContainerPort,
                name: TempoResource.GrpcEndpointName,
                isProxied: proxyEnabled,
                isExternal: false,
                scheme: "tcp"
            )
            .WithEndpoint(
                port: proxyOrContainerHostOtlpGrpcPort,
                targetPort: TempoResource.OtlpGrpcContainerPort,
                name: TempoResource.OtlpGrpcEndpointName,
                isProxied: proxyEnabled,
                isExternal: false,
                scheme: "tcp"
            )
            .WithEndpoint(
                port: proxyOrContainerHostOtlpHttpPort,
                targetPort: TempoResource.OtlpHttpContainerPort,
                name: TempoResource.OtlpHttpEndpointName,
                isProxied: proxyEnabled,
                isExternal: false,
                scheme: "http"
            )
            .WithHealthCheck(healthCheckKey);

        if (!string.IsNullOrEmpty(configBindMountPath))
        {
            tempo.WithConfigBindMount(configBindMountPath);
        }

        if (builder.ExecutionContext.IsPublishMode || persistenceEnabled)
        {
            tempo.WithLifetime(ContainerLifetime.Persistent);
            tempo.WithDataVolume("tempo_data");
        }

        tempo.WithEndpointProxySupport(proxyEnabled);

        return tempo;
    }

    public static IResourceBuilder<TempoResource> WithDataVolume(
        this IResourceBuilder<TempoResource> builder,
        string? name = null
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.WithVolume(
            name ?? VolumeNameGenerator.Generate(builder, "data"),
            TempoResource.DataTargetFolder
        );
    }

    public static IResourceBuilder<TempoResource> WithDataBindMount(
        this IResourceBuilder<TempoResource> builder,
        string source
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(source);
        return builder.WithBindMount(source, TempoResource.DataTargetFolder);
    }

    public static IResourceBuilder<TempoResource> WithConfigBindMount(
        this IResourceBuilder<TempoResource> builder,
        string source
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(source);
        return builder
            .WithBindMount(source, TempoResource.ConfigTargetPath)
            .WithArgs($"-config.file={TempoResource.ConfigTargetPath}");
    }
}
