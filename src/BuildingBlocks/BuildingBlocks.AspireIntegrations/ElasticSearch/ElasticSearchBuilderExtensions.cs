using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.AspireIntegrations.ElasticSearch;

[Experimental("ASPIREPROXYENDPOINTS001")]
public static class ElasticSearchBuilderExtensions
{
    /// <summary>
    /// Configures an Elasticsearch server as an Aspire resource with flexible port binding options.
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
    ///       Aspire generates a random host port for the container and uses the specified port as the proxy port.
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
    ///       The container binds directly to the specified host port (no proxy involved).
    ///       Traffic is routed directly to the container through this fixed host port.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term>Direct with dynamic port</term>
    ///     <term>false</term>
    ///     <term>null</term>
    ///     <description>
    ///       The container binds to a random available host port (no proxy involved).
    ///       Traffic is routed directly to the container through this random host port.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    /// <param name="builder">The distributed application builder</param>
    /// <param name="nameOrConnectionStringName">Name of the resource or connection string key</param>
    /// <param name="useExistingServerInstance">
    /// When true and in publish mode, references an existing Elasticsearch instance instead of creating a container
    /// </param>
    /// <param name="proxyEnabled">
    /// Enables Aspire's proxy for endpoint routing. When true, host ports are not exposed directly.
    /// </param>
    /// <param name="persistenceEnabled">
    /// Configures the container with persistent storage when true
    /// </param>
    /// <param name="proxyOrContainerHostHttpPort">
    /// Port for the HTTP API (9200). Behavior depends on proxyEnabled:
    /// - When proxyEnabled: Proxy port (null for auto-assigned)
    /// - When !proxyEnabled: Host port (null for auto-assigned)
    /// </param>
    /// <param name="proxyOrContainerHostTransportPort">
    /// Port for the transport protocol (9300). Behavior depends on proxyEnabled.
    /// </param>
    /// <param name="memoryLimitMB">
    /// Memory allocation in MB for Elasticsearch (default: 512MB)
    /// </param>
    /// <param name="disableSecurity">
    /// When true, disables Elasticsearch security features (development only)
    /// </param>
    /// <param name="clusterName">
    /// Name of the Elasticsearch cluster (default: "docker-cluster")
    /// </param>
    /// <param name="nodeName">
    /// Name of this Elasticsearch node (default: "docker-node")
    /// </param>
    /// <returns>An IResourceBuilder for the configured Elasticsearch resource</returns>
    public static IResourceBuilder<IResourceWithConnectionString> AddAspireElasticsearch(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string nameOrConnectionStringName,
        bool useExistingServerInstance = false,
        bool proxyEnabled = true,
        bool persistenceEnabled = false,
        int? proxyOrContainerHostHttpPort = ElasticsearchResource.ProxyOrContainerHostHttpPort,
        int? proxyOrContainerHostTransportPort = ElasticsearchResource.ProxyOrContainerHostTransportPort,
        int memoryLimitMB = 512,
        bool disableSecurity = true,
        string clusterName = "docker-cluster",
        string nodeName = "docker-node"
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(nameOrConnectionStringName);

        if (useExistingServerInstance && builder.ExecutionContext.IsPublishMode)
        {
            return builder.AddConnectionString(nameOrConnectionStringName);
        }

        var elasticsearchResource = new ElasticsearchResource(nameOrConnectionStringName);

        string? connectionString = null;

        builder.Eventing.Subscribe<ConnectionStringAvailableEvent>(
            elasticsearchResource,
            async (@event, cancellationToken) =>
            {
                connectionString =
                    await elasticsearchResource
                        .ConnectionStringExpression.GetValueAsync(cancellationToken)
                        .ConfigureAwait(false)
                    ?? throw new DistributedApplicationException(
                        $"ConnectionStringAvailableEvent was published for the '{elasticsearchResource.Name}' resource but the connection string was null."
                    );
            }
        );

        var healthCheckKey = $"{nameOrConnectionStringName}_check";
        builder
            .Services.AddHealthChecks()
            .Add(
                new HealthCheckRegistration(
                    healthCheckKey,
                    _ => new ElasticsearchHealthCheck(elasticsearchResource, connectionString!),
                    failureStatus: default,
                    tags: default,
                    timeout: default
                )
            );

        var elasticsearch = builder
            .AddResource(elasticsearchResource)
            .WithImage(ElasticsearchContainerImageTags.Image, ElasticsearchContainerImageTags.Tag)
            .WithImageRegistry(ElasticsearchContainerImageTags.Registry)
            .WithContainerName(nameOrConnectionStringName)
            .WithDataVolume()
            // HTTP API endpoint
            .WithHttpEndpoint(
                port: proxyOrContainerHostHttpPort,
                targetPort: ElasticsearchResource.HttpContainerPort,
                name: ElasticsearchResource.HttpEndpointName,
                isProxied: proxyEnabled
            // `isExternal` as default is `null` and will be assigned to `false` at the end when it's null
            )
            // Transport endpoint
            .WithEndpoint(
                port: proxyOrContainerHostTransportPort,
                targetPort: ElasticsearchResource.TransportContainerPort,
                name: ElasticsearchResource.TransportEndpointName,
                isProxied: proxyEnabled,
                scheme: "tcp",
                isExternal: false
            )
            .WithEnvironment(context =>
                ConfigureEnvironments(memoryLimitMB, disableSecurity, clusterName, nodeName, context)
            )
            .WithHealthCheck(healthCheckKey);

        if (builder.ExecutionContext.IsPublishMode || persistenceEnabled)
        {
            elasticsearch.WithLifetime(ContainerLifetime.Persistent);
            elasticsearch.WithDataVolume("elasticsearch_data");
        }

        elasticsearch.WithEndpointProxySupport(proxyEnabled);

        return elasticsearch;
    }

    private static void ConfigureEnvironments(
        int memoryLimitMb,
        bool disableSecurity,
        string clusterName,
        string nodeName,
        EnvironmentCallbackContext context
    )
    {
        context.EnvironmentVariables.Add("discovery.type", "single-node");
        context.EnvironmentVariables.Add("cluster.name", clusterName);
        context.EnvironmentVariables.Add("node.name", nodeName);
        context.EnvironmentVariables.Add("ES_JAVA_OPTS", $"-Xms{memoryLimitMb}m -Xmx{memoryLimitMb}m");
        context.EnvironmentVariables.Add("network.host", "0.0.0.0");
        context.EnvironmentVariables.Add("transport.host", "localhost");
        context.EnvironmentVariables.Add("bootstrap.memory_lock", "true");
        context.EnvironmentVariables.Add("cluster.routing.allocation.disk.threshold_enabled", "false");

        if (disableSecurity)
        {
            context.EnvironmentVariables.Add("xpack.security.enabled", "false");
            context.EnvironmentVariables.Add("xpack.security.http.ssl.enabled", "false");
            context.EnvironmentVariables.Add("xpack.security.transport.ssl.enabled", "false");
        }
    }

    public static IResourceBuilder<ElasticsearchResource> WithDataVolume(
        this IResourceBuilder<ElasticsearchResource> builder,
        string? name = null
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.WithVolume(
            name ?? VolumeNameGenerator.Generate(builder, "data"),
            ElasticsearchResource.DataTargetPath
        );
    }

    public static IResourceBuilder<ElasticsearchResource> WithDataBindMount(
        this IResourceBuilder<ElasticsearchResource> builder,
        string source
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(source);
        return builder.WithBindMount(source, ElasticsearchResource.DataTargetPath);
    }
}
