using System.Diagnostics.CodeAnalysis;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace BuildingBlocks.AspireIntegrations.RabbitMQ;

[Experimental("ASPIREPROXYENDPOINTS001")]
public static class RabbitMQBuilderExtensions
{
    /// <summary>
    /// Configures a RabbitMQ message broker as an Aspire resource with flexible port binding options.
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
    ///       Aspire generates random host ports for the container and uses the specified ports (typically 5672/15672) as proxy ports.
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
    ///       The container binds directly to the specified host ports (typically 5672/15672) with no proxy involved.
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
    /// Note: RabbitMQ exposes two ports - AMQP (5672) for messaging and Management (15672) for the web UI.
    /// Each port follows the same proxy/host binding behavior described above.
    /// </para>
    /// </remarks>
    /// <param name="builder">The distributed application builder</param>
    /// <param name="nameOrConnectionStringName">Name of the resource or connection string key (optional)</param>
    /// <param name="useExistingServerInstance">
    /// When true and in publish mode, references an existing RabbitMQ instance instead of creating a container (default: false)
    /// </param>
    /// <param name="proxyEnabled">
    /// Enables Aspire's proxy for AMQP endpoint routing. When true, host ports are not exposed directly (default: true)
    /// </param>
    /// <param name="persistenceEnabled">
    /// Configures the container with persistent storage when true (default: false)
    /// </param>
    /// <param name="proxyOrContainerHostPort">
    /// Port for AMQP messaging (5672). Behavior depends on proxyEnabled:
    /// - When proxyEnabled: Proxy port (null for auto-assigned)
    /// - When !proxyEnabled: Host port (null for auto-assigned)
    /// </param>
    /// <param name="proxyOrContainerManagementPort">
    /// Port for Management UI (15672). Behavior depends on managementUiProxyEnabled:
    /// - When managementUiProxyEnabled: Proxy port (null for auto-assigned)
    /// - When !managementUiProxyEnabled: Host port (null for auto-assigned)
    /// </param>
    /// <param name="pluginsPath">
    /// Optional path to custom RabbitMQ plugins directory to mount into the container
    /// </param>
    /// <param name="rabbitMqUser">
    /// RabbitMQ admin username as a ParameterResource (default: creates a parameter with value "guest")
    /// </param>
    /// <param name="rabbitMqPassword">
    /// RabbitMQ admin password as a ParameterResource (default: creates a parameter with value "guest")
    /// </param>
    /// <returns>An IResourceBuilder for the configured RabbitMQ resource</returns>
    public static IResourceBuilder<IResourceWithConnectionString> AddAspireRabbitmq(
        this IDistributedApplicationBuilder builder,
        string? nameOrConnectionStringName = null,
        bool useExistingServerInstance = false,
        bool proxyEnabled = true,
        bool persistenceEnabled = false,
        int? proxyOrContainerHostPort = RabbitMQDefaults.ProxyOrContainerHostAmqpPort,
        int? proxyOrContainerManagementPort = RabbitMQDefaults.ProxyOrContainerHostManagementPort,
        string? pluginsPath = null,
        IResourceBuilder<ParameterResource>? rabbitMqUser = null,
        IResourceBuilder<ParameterResource>? rabbitMqPassword = null
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(nameOrConnectionStringName);

        if (useExistingServerInstance && builder.ExecutionContext.IsPublishMode)
        {
            return builder.AddConnectionString(nameOrConnectionStringName);
        }

        var rabbitmq = builder
            .AddRabbitMQ(nameOrConnectionStringName, rabbitMqUser, rabbitMqPassword, port: proxyOrContainerHostPort)
            .WithImagePullPolicy(ImagePullPolicy.Missing)
            .WithImage(RabbitMQDefaults.ContainerImageName, RabbitMQDefaults.ContainerImageTag)
            .WithContainerName(nameOrConnectionStringName)
            .WithManagementPlugin(proxyOrContainerManagementPort)
            // config existing endpoint using endpoint name
            .WithEndpoint(
                endpointName: "tcp",
                callback: endpoint =>
                {
                    endpoint.TargetPort = RabbitMQDefaults.AmqpContainerPort;
                    endpoint.IsProxied = proxyEnabled;
                    endpoint.Port = proxyOrContainerHostPort;
                    endpoint.IsExternal = false;
                }
            )
            .WithEndpoint(
                endpointName: "management",
                callback: endpoint =>
                {
                    endpoint.TargetPort = RabbitMQDefaults.ManagementContainerPort;
                    endpoint.IsProxied = proxyEnabled;
                    endpoint.Port = proxyOrContainerManagementPort;
                    endpoint.IsExternal = true;
                }
            );

        if (!string.IsNullOrEmpty(pluginsPath))
        {
            rabbitmq.WithPluginsBindMount(pluginsPath);
        }

        if (builder.ExecutionContext.IsPublishMode || persistenceEnabled)
        {
            rabbitmq.WithLifetime(ContainerLifetime.Persistent);
            rabbitmq.WithDataVolume("rabbitmq_data");
        }

        return rabbitmq;
    }

    public static IResourceBuilder<RabbitMQServerResource> WithPluginsBindMount(
        this IResourceBuilder<RabbitMQServerResource> builder,
        string source
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(source);
        return builder
            .WithBindMount(source, RabbitMQDefaults.PluginsTargetPath)
            .WithEnvironment(context =>
            {
                context.EnvironmentVariables.Add(
                    "RABBITMQ_PLUGINS_DIR",
                    $"/opt/rabbitmq/plugins:{RabbitMQDefaults.PluginsTargetPath}"
                );
                context.EnvironmentVariables.Add(
                    "RABBITMQ_ENABLED_PLUGINS_FILE",
                    $"{RabbitMQDefaults.PluginsTargetPath}/rabbitmq_enabled_plugins"
                );
            });
    }

    private static class RabbitMQDefaults
    {
        public const string ContainerImageName = "library/rabbitmq";
        public const string ContainerImageTag = "management-alpine";

        public const string DefaultResourceName = "rabbitmq";
        public const string PluginsTargetPath = "/additional-plugins";

        public const int AmqpContainerPort = 5672;
        public const int ProxyOrContainerHostAmqpPort = 5672;

        public const int ManagementContainerPort = 15672;
        public const int ProxyOrContainerHostManagementPort = 15672;
    }
}
