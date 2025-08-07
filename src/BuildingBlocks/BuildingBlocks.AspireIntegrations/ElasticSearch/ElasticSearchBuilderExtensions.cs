using System.Diagnostics.CodeAnalysis;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

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
        int? proxyOrContainerHostHttpPort = ElasticSearchDefaults.ProxyOrContainerHostHttpPort,
        int? proxyOrContainerHostTransportPort = ElasticSearchDefaults.ProxyOrContainerHostTransportPort,
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

        var elasticsearch = builder
            .AddElasticsearch(nameOrConnectionStringName)
            .WithImage(ElasticSearchDefaults.Image, ElasticSearchDefaults.Tag)
            .WithImageRegistry(ElasticSearchDefaults.Registry)
            .WithContainerName(nameOrConnectionStringName)
            // HTTP API endpoint
            .WithEndpoint(
                ElasticSearchDefaults.HttpEndpointName,
                endpoint =>
                {
                    endpoint.Port = proxyOrContainerHostHttpPort;
                    endpoint.TargetPort = ElasticSearchDefaults.HttpContainerPort;
                    endpoint.IsProxied = proxyEnabled;
                    endpoint.IsExternal = false;
                }
            )
            // Transport endpoint
            .WithEndpoint(
                ElasticSearchDefaults.TransportEndpointName,
                endpoint =>
                {
                    endpoint.Port = proxyOrContainerHostTransportPort;
                    endpoint.TargetPort = ElasticSearchDefaults.TransportContainerPort;
                    endpoint.IsProxied = proxyEnabled;
                    endpoint.IsExternal = false;
                }
            )
            .WithEnvironment(context =>
                ConfigureEnvironments(memoryLimitMB, disableSecurity, clusterName, nodeName, context)
            );

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
        context.EnvironmentVariables.Add("cluster.name", clusterName);
        context.EnvironmentVariables.Add("node.name", nodeName);
        context.EnvironmentVariables.Add("ES_JAVA_OPTS", $"-Xms{memoryLimitMb}m -Xmx{memoryLimitMb}m");
        context.EnvironmentVariables.Add("network.host", "0.0.0.0");
        context.EnvironmentVariables.Add("transport.host", "localhost");
        context.EnvironmentVariables.Add("bootstrap.memory_lock", "true");
        context.EnvironmentVariables.Add("cluster.routing.allocation.disk.threshold_enabled", "false");

        if (disableSecurity)
        {
            context.EnvironmentVariables.Add("xpack.security.http.ssl.enabled", "false");
            context.EnvironmentVariables.Add("xpack.security.transport.ssl.enabled", "false");
        }
    }

    private static class ElasticSearchDefaults
    {
        internal const string Registry = "docker.elastic.co";
        internal const string Image = "elasticsearch/elasticsearch";
        internal const string Tag = "9.0.4";

        internal const string HttpEndpointName = "http";
        internal const string TransportEndpointName = "internal";

        public const string DefaultResourceName = "elasticsearch";

        public const int HttpContainerPort = 9200;
        public const int ProxyOrContainerHostHttpPort = 9200;

        public const int TransportContainerPort = 9300;
        public const int ProxyOrContainerHostTransportPort = 9300;
    }
}
