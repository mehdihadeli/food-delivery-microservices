using System.Diagnostics.CodeAnalysis;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.AspireIntegrations.Zipkin;

[Experimental("ASPIREPROXYENDPOINTS001")]
public static class ZipkinBuilderExtensions
{
    /// <summary>
    /// Configures a Zipkin distributed tracing backend as an Aspire resource with flexible port binding options.
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
    ///       Aspire generates a random host port for the container and uses the specified port (typically 9411) as the proxy port.
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
    ///       The container binds directly to the specified host port (typically 9411) with no proxy involved.
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
    /// When true and in publish mode, references an existing Zipkin instance instead of creating a container (default: false)
    /// </param>
    /// <param name="proxyEnabled">
    /// Enables Aspire's proxy for endpoint routing. When true, host ports are not exposed directly (default: true)
    /// </param>
    /// <param name="persistenceEnabled">
    /// Configures the container with persistent storage when true (default: false)
    /// </param>
    /// <param name="proxyOrContainerHostPort">
    /// Port for the Zipkin UI and API (9411). Behavior depends on proxyEnabled:
    /// - When proxyEnabled: Proxy port (null for auto-assigned)
    /// - When !proxyEnabled: Host port (null for auto-assigned)
    /// </param>
    /// <returns>An IResourceBuilder for the configured Zipkin resource</returns>
    public static IResourceBuilder<IResourceWithConnectionString> AddAspireZipkin(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string nameOrConnectionStringName,
        bool useExistingServerInstance = false,
        bool proxyEnabled = true,
        bool persistenceEnabled = false,
        int? proxyOrContainerHostPort = ZipkinResource.ProxyOrContainerHostPort
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(nameOrConnectionStringName);

        if (useExistingServerInstance && builder.ExecutionContext.IsPublishMode)
        {
            return builder.AddConnectionString(nameOrConnectionStringName);
        }

        var zipkinResource = new ZipkinResource(nameOrConnectionStringName);

        builder.Eventing.Subscribe<ConnectionStringAvailableEvent>(
            zipkinResource,
            async (@event, cancellationToken) =>
            {
                var connectionString =
                    await zipkinResource
                        .ConnectionStringExpression.GetValueAsync(cancellationToken)
                        .ConfigureAwait(false)
                    ?? throw new DistributedApplicationException(
                        $"ConnectionStringAvailableEvent was published for the '{zipkinResource.Name}' resource but the connection string was null."
                    );
            }
        );

        var healthCheckKey = $"{nameOrConnectionStringName}_check";
        builder
            .Services.AddHealthChecks()
            .Add(
                new HealthCheckRegistration(
                    healthCheckKey,
                    _ => new ZipkinHealthCheck(zipkinResource),
                    failureStatus: default,
                    tags: default,
                    timeout: default
                )
            );

        var zipkin = builder
            .AddResource(zipkinResource)
            .WithImage(ZipkinContainerImageTags.Image, ZipkinContainerImageTags.Tag)
            .WithImageRegistry(ZipkinContainerImageTags.Registry)
            .WithContainerName(nameOrConnectionStringName)
            .WithEndpoint(
                port: proxyOrContainerHostPort,
                targetPort: ZipkinResource.ContainerPort,
                name: ZipkinResource.EndpointName,
                isProxied: proxyEnabled,
                scheme: "http",
                isExternal: true
            )
            .WithHealthCheck(healthCheckKey);

        if (builder.ExecutionContext.IsPublishMode || persistenceEnabled)
        {
            zipkin.WithLifetime(ContainerLifetime.Persistent);
            zipkin.WithDataVolume("zipkin_data");
        }

        zipkin.WithEndpointProxySupport(proxyEnabled);

        return zipkin;
    }

    public static IResourceBuilder<ZipkinResource> WithDataVolume(
        this IResourceBuilder<ZipkinResource> builder,
        string? name = null
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.WithVolume(
            name ?? VolumeNameGenerator.Generate(builder, "data"),
            ZipkinResource.DataTargetFolder
        );
    }

    public static IResourceBuilder<ZipkinResource> WithDataBindMount(
        this IResourceBuilder<ZipkinResource> builder,
        string source
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(source);
        return builder.WithBindMount(source, ZipkinResource.DataTargetFolder);
    }

    private static void ConfigureZipkinContainer(EnvironmentCallbackContext context)
    {
        // Default Zipkin configuration
        // Can be changed to "elasticsearch" or "mysql" for persistence
        context.EnvironmentVariables.Add("STORAGE_TYPE", "mem");
    }
}
