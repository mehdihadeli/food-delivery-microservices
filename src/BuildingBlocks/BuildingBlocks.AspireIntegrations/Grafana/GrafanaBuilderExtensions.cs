using System.Diagnostics.CodeAnalysis;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.AspireIntegrations.Grafana;

[Experimental("ASPIREPROXYENDPOINTS001")]
public static class GrafanaBuilderExtensions
{
    /// <summary>
    /// Configures a Grafana monitoring dashboard as an Aspire resource with flexible port binding options.
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
    ///       The proxy routes traffic from this fixed proxy port (typically 3000) to the container's random host port.
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
    ///       The container binds directly to the specified host port (typically 3000) with no proxy involved.
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
    /// When true and in publish mode, references an existing Grafana instance instead of creating a container
    /// </param>
    /// <param name="proxyEnabled">
    /// Enables Aspire's proxy for endpoint routing. When true, host ports are not exposed directly.
    /// </param>
    /// <param name="persistenceEnabled">
    /// Configures the container with persistent storage when true
    /// </param>
    /// <param name="proxyOrContainerHostPort">
    /// Port for the Grafana UI (3000). Behavior depends on proxyEnabled:
    /// - When proxyEnabled: Proxy port (null for auto-assigned)
    /// - When !proxyEnabled: Host port (null for auto-assigned)
    /// </param>
    /// <param name="adminUser">Admin username (default: "admin")</param>
    /// <param name="adminPassword">Admin password (default: "admin")</param>
    /// <param name="plugins">Comma-separated list of plugins to install (default: "grafana-clock-panel,grafana-simple-json-datasource")</param>
    /// <param name="enableTraceQLEditor">Enables TraceQL editor feature flag (default: true)</param>
    /// <param name="provisioningPath">
    /// Optional path to Grafana provisioning configuration directory
    /// </param>
    /// <param name="dashboardsPath">
    /// Optional path to Grafana dashboards directory
    /// </param>
    /// <param name="configPath">
    /// Optional path to custom grafana.ini configuration file
    /// </param>
    /// <param name="waitForDependencies">Optional array of resources that Grafana should wait for before starting</param>
    /// <returns>An IResourceBuilder for the configured Grafana resource</returns>
    public static IResourceBuilder<IResourceWithConnectionString> AddAspireGrafana(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string nameOrConnectionStringName,
        bool useExistingServerInstance = false,
        bool proxyEnabled = true,
        bool persistenceEnabled = false,
        int? proxyOrContainerHostPort = GrafanaResource.ProxyOrContainerHostPort,
        string adminUser = "admin",
        string adminPassword = "admin",
        string? plugins = "grafana-clock-panel,grafana-simple-json-datasource",
        bool enableTraceQLEditor = true,
        string? provisioningPath = null,
        string? dashboardsPath = null,
        string? configPath = null,
        IResourceBuilder<IResourceWithConnectionString>[]? waitForDependencies = null
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(nameOrConnectionStringName);

        if (useExistingServerInstance && builder.ExecutionContext.IsPublishMode)
        {
            return builder.AddConnectionString(nameOrConnectionStringName);
        }

        var grafanaResource = new GrafanaResource(nameOrConnectionStringName);

        var healthCheckKey = $"{nameOrConnectionStringName}_check";
        builder
            .Services.AddHealthChecks()
            .Add(
                new HealthCheckRegistration(
                    healthCheckKey,
                    _ => new GrafanaHealthCheck(grafanaResource),
                    failureStatus: default,
                    tags: default,
                    timeout: default
                )
            );

        var grafana = builder
            .AddResource(grafanaResource)
            .WithImage(GrafanaContainerImageTags.Image, GrafanaContainerImageTags.Tag)
            .WithImageRegistry(GrafanaContainerImageTags.Registry)
            .WithContainerName(nameOrConnectionStringName)
            .WithEndpoint(
                port: proxyOrContainerHostPort,
                targetPort: GrafanaResource.ContainerPort,
                scheme: "http",
                name: GrafanaResource.PrimaryEndpointName,
                isProxied: proxyEnabled,
                isExternal: true
            )
            .WithEnvironment(ctx => ConfigureEnvironments(ctx, adminUser, adminPassword, plugins, enableTraceQLEditor))
            .WithHealthCheck(healthCheckKey);

        if (!string.IsNullOrEmpty(provisioningPath))
        {
            grafana.WithProvisioningBindMount(provisioningPath);
        }

        if (!string.IsNullOrEmpty(dashboardsPath))
        {
            grafana.WithDashboardsBindMount(dashboardsPath);
        }

        if (!string.IsNullOrEmpty(configPath))
        {
            grafana.WithConfigBindMount(configPath);
        }

        if (builder.ExecutionContext.IsPublishMode || persistenceEnabled)
        {
            grafana.WithLifetime(ContainerLifetime.Persistent);
            grafana.WithDataVolume("grafana_data");
        }

        waitForDependencies?.ToList().ForEach(x => grafana.WaitFor(x));

        grafana.WithEndpointProxySupport(proxyEnabled);

        return grafana;
    }

    private static void ConfigureEnvironments(
        EnvironmentCallbackContext context,
        string adminUser,
        string adminPassword,
        string? plugins,
        bool enableTraceQlEditor
    )
    {
        context.EnvironmentVariables.Add("GF_SECURITY_ADMIN_USER", adminUser);
        context.EnvironmentVariables.Add("GF_SECURITY_ADMIN_PASSWORD", adminPassword);

        if (!string.IsNullOrEmpty(plugins))
        {
            context.EnvironmentVariables.Add("GF_INSTALL_PLUGINS", plugins);
        }

        if (enableTraceQlEditor)
        {
            context.EnvironmentVariables.Add("GF_FEATURE_TOGGLES_ENABLE", "traceqlEditor");
        }
    }

    public static IResourceBuilder<GrafanaResource> WithDataVolume(
        this IResourceBuilder<GrafanaResource> builder,
        string? name = null
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.WithVolume(
            name ?? VolumeNameGenerator.Generate(builder, "data"),
            GrafanaResource.DataTargetFolder
        );
    }

    public static IResourceBuilder<GrafanaResource> WithDataBindMount(
        this IResourceBuilder<GrafanaResource> builder,
        string source
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(source);
        return builder.WithBindMount(source, GrafanaResource.DataTargetFolder);
    }

    public static IResourceBuilder<GrafanaResource> WithProvisioningBindMount(
        this IResourceBuilder<GrafanaResource> builder,
        string source
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(source);
        return builder.WithBindMount(source, GrafanaResource.ProvisioningTargetPath);
    }

    public static IResourceBuilder<GrafanaResource> WithDashboardsBindMount(
        this IResourceBuilder<GrafanaResource> builder,
        string source
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(source);
        return builder.WithBindMount(source, GrafanaResource.DashboardsTargetPath);
    }

    public static IResourceBuilder<GrafanaResource> WithConfigBindMount(
        this IResourceBuilder<GrafanaResource> builder,
        string source
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(source);
        return builder.WithBindMount(source, GrafanaResource.ConfigTargetPath);
    }
}
