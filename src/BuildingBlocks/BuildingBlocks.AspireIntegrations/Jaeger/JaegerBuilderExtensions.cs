using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.AspireIntegrations.Jaeger;

[Experimental("ASPIREPROXYENDPOINTS001")]
public static class JaegerBuilderExtensions
{
    /// <summary>
    /// Configures a Jaeger distributed tracing backend as an Aspire resource with flexible port binding options.
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
    ///       The container binds directly to the specified host ports (no proxy involved).
    ///       Traffic is routed directly to the container through these fixed host ports.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term>Direct with dynamic port</term>
    ///     <term>false</term>
    ///     <term>null</term>
    ///     <description>
    ///       The container binds to random available host ports (no proxy involved).
    ///       Traffic is routed directly to the container through these random host ports.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    /// <param name="builder">The distributed application builder</param>
    /// <param name="nameOrConnectionStringName">Name of the resource or connection string key</param>
    /// <param name="useExistingServerInstance">
    /// When true and in publish mode, references an existing Jaeger instance instead of creating a container
    /// </param>
    /// <param name="proxyEnabled">
    /// Enables Aspire's proxy for endpoint routing. When true, host ports are not exposed directly.
    /// </param>
    /// <param name="persistenceEnabled">
    /// Configures the container with persistent storage when true
    /// </param>
    /// <param name="proxyOrContainerHostHttpPort">
    /// Port for the Jaeger UI (16686). Behavior depends on proxyEnabled
    /// </param>
    /// <param name="proxyOrContainerHostAgentPort">
    /// Port for the Jaeger agent (6831). Behavior depends on proxyEnabled:
    /// - When proxyEnabled: Proxy port (null for auto-assigned)
    /// - When !proxyEnabled: Host port (null for auto-assigned)
    /// </param>
    /// <param name="proxyOrContainerHostCollectorPort">
    /// Port for the Jaeger collector (14268). Behavior depends on proxyEnabled:
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
    /// <returns>An IResourceBuilder for the configured Jaeger resource</returns>
    public static IResourceBuilder<IResourceWithConnectionString> AddAspireJaeger(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string nameOrConnectionStringName,
        bool useExistingServerInstance = false,
        bool proxyEnabled = true,
        bool persistenceEnabled = false,
        int? proxyOrContainerHostHttpPort = JaegerResource.ProxyOrContainerHostQueryHttpPort,
        int? proxyOrContainerHostAgentPort = JaegerResource.ProxyOrContainerHostAgentPort,
        int? proxyOrContainerHostCollectorPort = JaegerResource.ProxyOrContainerHostCollectorPort,
        int? proxyOrContainerHostOtlpGrpcPort = JaegerResource.ProxyOrContainerHostOtlpGrpcPort,
        int? proxyOrContainerHostOtlpHttpPort = JaegerResource.ProxyOrContainerHostOtlpHttpPort
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(nameOrConnectionStringName);

        if (useExistingServerInstance && builder.ExecutionContext.IsPublishMode)
        {
            return builder.AddConnectionString(nameOrConnectionStringName);
        }

        var jaegerResource = new JaegerResource(nameOrConnectionStringName);

        builder.Eventing.Subscribe<ConnectionStringAvailableEvent>(
            jaegerResource,
            async (@event, cancellationToken) =>
            {
                var connectionString =
                    await jaegerResource
                        .ConnectionStringExpression.GetValueAsync(cancellationToken)
                        .ConfigureAwait(false)
                    ?? throw new DistributedApplicationException(
                        $"ConnectionStringAvailableEvent was published for the '{jaegerResource.Name}' resource but the connection string was null."
                    );
            }
        );

        var healthCheckKey = $"{nameOrConnectionStringName}_check";
        builder
            .Services.AddHealthChecks()
            .Add(
                new HealthCheckRegistration(
                    healthCheckKey,
                    _ => new JaegerHealthCheck(jaegerResource),
                    failureStatus: default,
                    tags: default,
                    timeout: default
                )
            );

        var jaeger = builder
            .AddResource(jaegerResource)
            .WithImage(JaegerContainerImageTags.Image, JaegerContainerImageTags.Tag)
            .WithImageRegistry(JaegerContainerImageTags.Registry)
            .WithContainerName(nameOrConnectionStringName)
            // UI endpoint
            .WithEndpoint(
                port: proxyOrContainerHostHttpPort,
                targetPort: JaegerResource.QueryHttpContainerPort,
                name: JaegerResource.QueryHttpEndpointName,
                isProxied: proxyEnabled,
                scheme: "http",
                isExternal: true
            )
            // Agent endpoint (UDP)
            .WithEndpoint(
                port: proxyOrContainerHostAgentPort,
                targetPort: JaegerResource.AgentContainerPort,
                name: JaegerResource.AgentEndpointName,
                isProxied: proxyEnabled,
                protocol: ProtocolType.Udp
            )
            // Collector endpoint
            .WithHttpEndpoint(
                port: proxyOrContainerHostCollectorPort,
                targetPort: JaegerResource.CollectorContainerPort,
                name: JaegerResource.CollectorEndpointName,
                isProxied: proxyEnabled
            )
            // OTLP endpoints
            .WithEndpoint(
                port: proxyOrContainerHostOtlpGrpcPort,
                targetPort: JaegerResource.OtlpGrpcContainerPort,
                name: JaegerResource.OtlpGrpcEndpointName,
                isProxied: proxyEnabled,
                isExternal: false,
                scheme: "tcp"
            )
            .WithHttpEndpoint(
                port: proxyOrContainerHostOtlpHttpPort,
                targetPort: JaegerResource.OtlpHttpContainerPort,
                name: JaegerResource.OtlpHttpEndpointName,
                isProxied: proxyEnabled
            )
            .WithEnvironment(context =>
            {
                context.EnvironmentVariables.Add("COLLECTOR_OTLP_ENABLED", "true");
                context.EnvironmentVariables.Add("SPAN_STORAGE_TYPE", "memory");
            })
            .WithHealthCheck(healthCheckKey);

        if (builder.ExecutionContext.IsPublishMode || persistenceEnabled)
        {
            jaeger.WithLifetime(ContainerLifetime.Persistent);
            jaeger.WithDataVolume("jaeger_data");
        }

        return jaeger;
    }

    public static IResourceBuilder<JaegerResource> WithDataVolume(
        this IResourceBuilder<JaegerResource> builder,
        string? name = null
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.WithVolume(
            name ?? VolumeNameGenerator.Generate(builder, "data"),
            JaegerResource.DataTargetFolder
        );
    }

    public static IResourceBuilder<JaegerResource> WithDataBindMount(
        this IResourceBuilder<JaegerResource> builder,
        string source
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(source);
        return builder.WithBindMount(source, JaegerResource.DataTargetFolder);
    }

    private static void ConfigureJaegerContainer(EnvironmentCallbackContext context)
    {
        context.EnvironmentVariables.Add("COLLECTOR_OTLP_ENABLED", "true");
        context.EnvironmentVariables.Add("SPAN_STORAGE_TYPE", "memory");
    }
}
