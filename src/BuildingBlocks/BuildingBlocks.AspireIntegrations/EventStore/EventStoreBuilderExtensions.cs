using System.Diagnostics.CodeAnalysis;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace BuildingBlocks.AspireIntegrations.EventStore;

[Experimental("ASPIREPROXYENDPOINTS001")]
public static class EventStoreBuilderExtensions
{
    /// <summary>
    /// Configures an EventStoreDB server as an Aspire resource with flexible port binding options.
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
    /// When true and in publish mode, references an existing EventStoreDB instance instead of creating a container
    /// </param>
    /// <param name="proxyEnabled">
    /// Enables Aspire's proxy for endpoint routing. When true, host ports are not exposed directly.
    /// </param>
    /// <param name="persistenceEnabled">
    /// Configures the container with persistent storage when true (default: false for memory DB)
    /// </param>
    /// <param name="proxyOrContainerHostHttpPort">
    /// Port for HTTP API (2113). Behavior depends on proxyEnabled:
    /// - When proxyEnabled: Proxy port (null for auto-assigned)
    /// - When !proxyEnabled: Host port (null for auto-assigned)
    /// </param>
    /// <param name="proxyOrContainerHostTcpPort">
    /// Port for TCP protocol (1113). Behavior depends on proxyEnabled:
    /// - When proxyEnabled: Proxy port (null for auto-assigned)
    /// - When !proxyEnabled: Host port (null for auto-assigned)
    /// </param>
    /// <param name="runProjections">
    /// Configures which projections to run (default: "All")
    /// </param>
    /// <param name="startStandardProjections">
    /// Whether to start standard projections (default: false)
    /// </param>
    /// <param name="enableInsecureMode">
    /// Disables security for development (default: true)
    /// </param>
    /// <param name="useMemoryDb">
    /// Uses an in-memory database instead of persistence (default: true)
    /// </param>
    /// <param name="logPath">Path for EventStoreDB logs</param>
    /// <returns>An IResourceBuilder for the configured EventStoreDB resource</returns>
    public static IResourceBuilder<IResourceWithConnectionString> AddAspireEventStore(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string nameOrConnectionStringName,
        bool useExistingServerInstance = false,
        bool proxyEnabled = true,
        bool persistenceEnabled = false,
        int? proxyOrContainerHostHttpPort = EventStoreDefaults.ProxyOrContainerHostHttpPort,
        int? proxyOrContainerHostTcpPort = EventStoreDefaults.ProxyOrContainerHostTcpPort,
        string runProjections = "All",
        bool startStandardProjections = false,
        bool enableInsecureMode = true,
        bool useMemoryDb = true,
        string? logPath = null
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(nameOrConnectionStringName);

        if (useExistingServerInstance && builder.ExecutionContext.IsPublishMode)
        {
            return builder.AddConnectionString(nameOrConnectionStringName);
        }

        // https://learn.microsoft.com/en-us/dotnet/aspire/community-toolkit/hosting-eventstore?tabs=dotnet-cli
        var eventstore = builder
            .AddEventStore(nameOrConnectionStringName, port: proxyOrContainerHostHttpPort)
            .WithContainerName(nameOrConnectionStringName)
            .WithImagePullPolicy(ImagePullPolicy.Missing)
            .WithEndpoint(
                endpointName: "http",
                callback: endpoint =>
                {
                    endpoint.TargetPort = EventStoreDefaults.HttpContainerPort;
                    endpoint.IsProxied = proxyEnabled;
                    endpoint.Port = proxyOrContainerHostHttpPort;
                    endpoint.IsExternal = true;
                }
            )
            .WithEndpoint(
                port: proxyOrContainerHostTcpPort,
                targetPort: EventStoreDefaults.TcpContainerPort,
                scheme: "tcp",
                name: "tcp",
                isProxied: proxyEnabled,
                isExternal: false
            )
            .WithImage(EventStoreDefaults.ContainerImageName, EventStoreDefaults.ContainerImageTag)
            .WithEnvironment(ctx =>
                ConfigureEventStoreContainer(
                    ctx,
                    proxyOrContainerHostHttpPort,
                    useMemoryDb,
                    startStandardProjections,
                    enableInsecureMode,
                    runProjections
                )
            );

        if (builder.ExecutionContext.IsPublishMode || persistenceEnabled)
        {
            eventstore.WithLifetime(ContainerLifetime.Persistent);
            eventstore.WithDataVolume("eventstore_data");
        }

        if (!string.IsNullOrEmpty(logPath))
        {
            eventstore.WithLogsVolume(logPath);
        }

        // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/networking-overview#ports-and-proxies
        // https://github.com/dotnet/aspire/issues/1637
        // - aspire has a proxy for endpoints for 2 Reason:
        // 1. to avoid forwarding the connection until the underling endpoint is ready
        // 2. to route to multiple replicas of the underlying service
        //
        // - if proxy is disabled, this port will be the host port of the container, and if proxy is disabled by `WithEndpointProxySupport(false)`
        // this port will use by aspire proxy to route to a random generated host container port.
        eventstore.WithEndpointProxySupport(proxyEnabled);

        return eventstore;
    }

    public static IResourceBuilder<EventStoreResource> WithDataVolume(
        this IResourceBuilder<EventStoreResource> builder,
        string? name = null
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.WithVolume(
            name ?? VolumeNameGenerator.Generate(builder, "data"),
            EventStoreDefaults.DataTargetPath
        );
    }

    public static IResourceBuilder<EventStoreResource> WithLogsVolume(
        this IResourceBuilder<EventStoreResource> builder,
        string? name = null
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.WithVolume(
            name ?? VolumeNameGenerator.Generate(builder, "logs"),
            EventStoreDefaults.LogsTargetPath
        );
    }

    private static void ConfigureEventStoreContainer(
        EnvironmentCallbackContext context,
        int? httpPort,
        bool useMemoryDb,
        bool startStandardProjections,
        bool enableInsecureMode,
        string runProjections
    )
    {
        context.EnvironmentVariables["EVENTSTORE_RUN_PROJECTIONS"] = runProjections;
        context.EnvironmentVariables["EVENTSTORE_START_STANDARD_PROJECTIONS"] = startStandardProjections.ToString();
        context.EnvironmentVariables["EVENTSTORE_INSECURE"] = enableInsecureMode.ToString();
        context.EnvironmentVariables.Add("EVENTSTORE_HTTP_PORT", httpPort.ToString()!);
        context.EnvironmentVariables.Add("EVENTSTORE_ENABLE_ATOM_PUB_OVER_HTTP", "true");
        context.EnvironmentVariables.Add("EVENTSTORE_MEM_DB", useMemoryDb.ToString());
    }

    private static class EventStoreDefaults
    {
        public const string ContainerImageName = "eventstore/eventstore";
        public const string ContainerImageTag = "latest";

        public const string DefaultResourceName = "eventstore";
        public const string DataTargetPath = "/var/lib/eventstore";
        public const string LogsTargetPath = "/var/log/eventstore";

        public const int HttpContainerPort = 2113;
        public const int ProxyOrContainerHostHttpPort = 2113;

        public const int TcpContainerPort = 1113;
        public const int ProxyOrContainerHostTcpPort = 1113;
    }
}
