using System.Diagnostics.CodeAnalysis;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace BuildingBlocks.AspireIntegrations.Redis;

[Experimental("ASPIREPROXYENDPOINTS001")]
public static class RedisBuilderExtensions
{
    /// <summary>
    /// Configures a Redis server as an Aspire resource with flexible port binding options.
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
    ///       Aspire generates a random host port for the container and uses the specified port (typically 6379) as the proxy port.
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
    ///       The container binds directly to the specified host port (typically 6379) with no proxy involved.
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
    /// When true and in publish mode, references an existing Redis instance instead of creating a container (default: false)
    /// </param>
    /// <param name="proxyEnabled">
    /// Enables Aspire's proxy for endpoint routing. When true, host ports are not exposed directly (default: true)
    /// </param>
    /// <param name="persistenceEnabled">
    /// Configures the container with persistent storage when true (default: false)
    /// </param>
    /// <param name="proxyOrContainerHostPort">
    /// Port for Redis (6379). Behavior depends on proxyEnabled:
    /// - When proxyEnabled: Proxy port (null for auto-assigned)
    /// - When !proxyEnabled: Host port (null for auto-assigned)
    /// </param>
    /// <param name="password">
    /// Redis password as a ParameterResource (default: null - no password required)
    /// </param>
    /// <returns>An IResourceBuilder for the configured Redis resource</returns>
    public static IResourceBuilder<IResourceWithConnectionString> AddAspireRedis(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string nameOrConnectionStringName,
        bool useExistingServerInstance = false,
        bool proxyEnabled = true,
        bool persistenceEnabled = false,
        int? proxyOrContainerHostPort = RedisDefaults.ProxyOrContainerHostPort,
        IResourceBuilder<ParameterResource>? password = null
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(nameOrConnectionStringName);

        if (useExistingServerInstance && builder.ExecutionContext.IsPublishMode)
        {
            return builder.AddConnectionString(nameOrConnectionStringName);
        }

        var redis = builder
            .AddRedis(nameOrConnectionStringName, port: proxyOrContainerHostPort, password: password)
            .WithImagePullPolicy(ImagePullPolicy.Missing)
            .WithContainerName(nameOrConnectionStringName)
            .WithImage(RedisDefaults.ContainerImageName, RedisDefaults.ContainerImageTag)
            // config existing endpoint using endpoint name
            .WithEndpoint(
                endpointName: "tcp",
                callback: endpoint =>
                {
                    endpoint.TargetPort = RedisDefaults.ContainerPort;
                    endpoint.IsProxied = proxyEnabled;
                    endpoint.Port = proxyOrContainerHostPort;
                    endpoint.IsExternal = false;
                }
            );

        if (builder.ExecutionContext.IsPublishMode || persistenceEnabled)
        {
            redis.WithLifetime(ContainerLifetime.Persistent);
            redis.WithDataVolume("redis_data");
        }

        // // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/networking-overview#ports-and-proxies
        // // https://github.com/dotnet/aspire/issues/1637
        // // - aspire has a proxy for endpoints for 2 Reason:
        // // 1. to avoid forwarding the connection until the underling endpoint is ready
        // // 2. to route to multiple replicas of the underlying service
        // //
        // // - if proxy is disabled, this port will be the host port of the container, and if proxy is disabled by `WithEndpointProxySupport(false)`
        // // this port will use by aspire proxy to route to a random generated host container port.
        redis.WithEndpointProxySupport(proxyEnabled);

        return redis;
    }

    private static class RedisDefaults
    {
        public const string ContainerImageName = "redis/redis-stack";
        public const string ContainerImageTag = "latest";

        public const string DataTargetPath = "/data";

        public const int ContainerPort = 6379;
        public const int ProxyOrContainerHostPort = 6379;
    }
}
