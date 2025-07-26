using System.Diagnostics.CodeAnalysis;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.AspireIntegrations.Kibana;

[Experimental("ASPIREPROXYENDPOINTS001")]
public static class KibanaBuilderExtensions
{
    /// <summary>
    /// Configures a Kibana dashboard as an Aspire resource with flexible port binding options.
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
    ///       Aspire generates a random host port for the container and uses the specified port (typically 5601) as the proxy port.
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
    ///       The container binds directly to the specified host port (typically 5601) with no proxy involved.
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
    /// <para>
    /// Note: Kibana requires a connection to an Elasticsearch instance which should be configured first.
    /// </para>
    /// </remarks>
    /// <param name="builder">The distributed application builder</param>
    /// <param name="nameOrConnectionStringName">Name of the resource or connection string key</param>
    /// <param name="elasticsearch">Required reference to the Elasticsearch resource that Kibana will connect to</param>
    /// <param name="useExistingServerInstance">
    /// When true and in publish mode, references an existing Kibana instance instead of creating a container
    /// </param>
    /// <param name="proxyEnabled">
    /// Enables Aspire's proxy for endpoint routing. When true, host ports are not exposed directly (default: true)
    /// </param>
    /// <param name="persistenceEnabled">
    /// Configures the container with persistent storage when true (default: false)
    /// </param>
    /// <param name="proxyOrContainerHostPort">
    /// Port for the Kibana UI (5601). Behavior depends on proxyEnabled:
    /// - When proxyEnabled: Proxy port (null for auto-assigned)
    /// - When !proxyEnabled: Host port (null for auto-assigned)
    /// </param>
    /// <returns>An IResourceBuilder for the configured Kibana resource</returns>
    public static IResourceBuilder<IResource> AddAspireKibana(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string nameOrConnectionStringName,
        IResourceBuilder<IResourceWithConnectionString> elasticsearch,
        bool useExistingServerInstance = false,
        bool proxyEnabled = true,
        bool persistenceEnabled = false,
        int? proxyOrContainerHostPort = KibanaResource.ProxyOrContainerHostPort
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(nameOrConnectionStringName);
        ArgumentNullException.ThrowIfNull(elasticsearch);

        if (useExistingServerInstance && builder.ExecutionContext.IsPublishMode)
        {
            return builder.AddConnectionString(nameOrConnectionStringName);
        }

        var kibanaResource = new KibanaResource(nameOrConnectionStringName);

        string? connectionString = null;

        builder.Eventing.Subscribe<ConnectionStringAvailableEvent>(
            kibanaResource,
            async (@event, cancellationToken) =>
            {
                connectionString =
                    await kibanaResource
                        .ConnectionStringExpression.GetValueAsync(cancellationToken)
                        .ConfigureAwait(false)
                    ?? throw new DistributedApplicationException(
                        $"ConnectionStringAvailableEvent was published for the '{kibanaResource.Name}' resource but the connection string was null."
                    );
            }
        );

        var healthCheckKey = $"{nameOrConnectionStringName}_check";
        builder
            .Services.AddHealthChecks()
            .Add(
                new HealthCheckRegistration(
                    healthCheckKey,
                    _ => new KibanaHealthCheck(kibanaResource, connectionString!),
                    failureStatus: default,
                    tags: default,
                    timeout: default
                )
            );

        var kibana = builder
            .AddResource(kibanaResource)
            .WithImage(KibanaContainerImageTags.Image, KibanaContainerImageTags.Tag)
            .WithImageRegistry(KibanaContainerImageTags.Registry)
            .WithContainerName(nameOrConnectionStringName)
            .WithEndpoint(
                port: proxyOrContainerHostPort,
                targetPort: KibanaResource.ContainerPort,
                name: KibanaResource.HttpEndpointName,
                isProxied: proxyEnabled,
                isExternal: true,
                scheme: "http"
            )
            .WithEnvironment(context =>
            {
                context.EnvironmentVariables.Add("ELASTICSEARCH_HOSTS", $"http://{elasticsearch.Resource.Name}:9200");
            })
            .WithHealthCheck(healthCheckKey);

        if (builder.ExecutionContext.IsPublishMode || persistenceEnabled)
        {
            kibana.WithLifetime(ContainerLifetime.Persistent);
            kibana.WithDataVolume("kibana_data");
        }

        kibana.WaitFor(elasticsearch);

        kibana.WithEndpointProxySupport(proxyEnabled);

        return kibana;
    }

    public static IResourceBuilder<KibanaResource> WithDataVolume(
        this IResourceBuilder<KibanaResource> builder,
        string? name = null
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.WithVolume(name ?? VolumeNameGenerator.Generate(builder, "data"), KibanaResource.DataTargetPath);
    }

    public static IResourceBuilder<KibanaResource> WithDataBindMount(
        this IResourceBuilder<KibanaResource> builder,
        string source
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(source);
        return builder.WithBindMount(source, KibanaResource.DataTargetPath);
    }
}
