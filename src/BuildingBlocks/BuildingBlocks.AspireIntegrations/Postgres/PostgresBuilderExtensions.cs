using System.Diagnostics.CodeAnalysis;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace BuildingBlocks.AspireIntegrations.Postgres;

[Experimental("ASPIREPROXYENDPOINTS001")]
public static class PostgresBuilderExtensions
{
    /// <summary>
    /// Configures a PostgreSQL database server as an Aspire resource with flexible port binding options.
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
    ///       Aspire generates a random host port for the container and uses the specified port (typically 5432) as the proxy port.
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
    ///       The container binds directly to the specified host port (typically 5432) with no proxy involved.
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
    /// When true and in publish mode, references an existing PostgreSQL instance instead of creating a container (default: false)
    /// </param>
    /// <param name="proxyEnabled">
    /// Enables Aspire's proxy for endpoint routing. When true, host ports are not exposed directly (default: true)
    /// </param>
    /// <param name="persistenceEnabled">
    /// Configures the container with persistent storage when true (default: false)
    /// </param>
    /// <param name="proxyOrContainerHostPort">
    /// Port for PostgreSQL (5432). Behavior depends on proxyEnabled:
    /// - When proxyEnabled: Proxy port (null for auto-assigned)
    /// - When !proxyEnabled: Host port (null for auto-assigned)
    /// </param>
    /// <param name="userName">
    /// PostgreSQL admin username as a ParameterResource (default: creates a parameter with value "postgres")
    /// </param>
    /// <param name="password">
    /// PostgreSQL admin password as a ParameterResource (default: creates a parameter with value "postgres")
    /// </param>
    /// <param name="initScriptPath">
    /// Optional path to SQL initialization script that will run on first startup
    /// </param>
    /// <returns>
    /// A tuple containing:
    /// - postgresResource: The PostgreSQL resource builder (null when useExistingServerInstance is true)
    /// - applicationBuilder: The distributed application builder for method chaining
    /// </returns>
    public static (
        IResourceBuilder<PostgresServerResource>? postgresResource,
        IDistributedApplicationBuilder applicationBuilder
    ) AddAspirePostgres(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string nameOrConnectionStringName,
        bool useExistingServerInstance = false,
        bool proxyEnabled = true,
        bool persistenceEnabled = false,
        int? proxyOrContainerHostPort = PostgresDefaults.ProxyOrContainerHostPort,
        IResourceBuilder<ParameterResource>? userName = null,
        IResourceBuilder<ParameterResource>? password = null,
        string? initScriptPath = null
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(nameOrConnectionStringName);

        if (useExistingServerInstance && builder.ExecutionContext.IsPublishMode)
        {
            return (null, builder);
        }

        // https://learn.microsoft.com/en-us/dotnet/aspire/database/postgresql-entity-framework-integration
        // https://learn.microsoft.com/en-us/dotnet/aspire/database/postgresql-entity-framework-integration
        var postgres = builder
            .AddPostgres(
                name: nameOrConnectionStringName,
                userName: userName,
                password: password,
                port: proxyOrContainerHostPort
            )
            .WithImage(PostgresDefaults.ContainerImageName, PostgresDefaults.ContainerImageTag)
            .WithImagePullPolicy(ImagePullPolicy.Missing)
            .WithContainerName(nameOrConnectionStringName)
            // modify the existing endpoint with endpointName
            .WithEndpoint(
                endpointName: "tcp",
                callback: endpoint =>
                {
                    endpoint.TargetPort = PostgresDefaults.ContainerPort;
                    endpoint.IsProxied = proxyEnabled;
                    endpoint.Port = proxyOrContainerHostPort;
                    endpoint.IsExternal = false;
                }
            );

        if (builder.ExecutionContext.IsPublishMode || persistenceEnabled)
        {
            postgres.WithLifetime(ContainerLifetime.Persistent);
            postgres.WithDataVolume("postgres_data");
        }

        if (!string.IsNullOrEmpty(initScriptPath))
        {
            postgres.WithInitScriptBindMount(initScriptPath);
        }

        // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/networking-overview#ports-and-proxies
        // https://github.com/dotnet/aspire/issues/1637
        // - aspire has proxy for endpoints for 2 Reason:
        // 1. to avoid forwarding the connection until the underling endpoint is ready
        // 2. to route to multiple replicas of the underlying service
        //
        // - if proxy is disabled, this port will be the host port of the container, and if proxy is disabled by `WithEndpointProxySupport(false)`
        // this port will use by aspire proxy to route to a random generated host container port.
        postgres.WithEndpointProxySupport(proxyEnabled);

        return (postgres, builder);
    }

    public static IResourceBuilder<PostgresServerResource> WithInitScriptBindMount(
        this IResourceBuilder<PostgresServerResource> builder,
        string source
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(source);
        return builder.WithBindMount(source, PostgresDefaults.InitScriptTargetPath);
    }

    /// <summary>
    /// Adds a PostgreSQL database connection resource to the distributed application based on the provided server builder tuple.
    /// If a PostgreSQL server resource exists in the tuple, a new database is created on that server and a corresponding connection string resource is configured with the given name.
    /// If no server resource is provided, treats the given name as an existing connection string and registers it for connecting to an external or pre-existing database server instance.
    /// </summary>
    /// <param name="builder">
    /// A tuple containing an optional PostgreSQL server resource and the distributed application builder.
    /// </param>
    /// <param name="nameOrConnectionStringName">
    /// The name of a postgres database resource (if creating one) or the key/name of an existing connection string for remote/external servers.
    /// </param>
    /// <param name="databaseName">database name</param>
    /// <returns>
    /// A resource builder for the configured PostgreSQL database connection, either newly created or from an existing connection string.
    /// </returns>
    public static IResourceBuilder<IResourceWithConnectionString> AddAspirePostgresDatabase(
        this (
            IResourceBuilder<PostgresServerResource>? postgresResource,
            IDistributedApplicationBuilder applicationBuilder
        ) builder,
        string nameOrConnectionStringName,
        string? databaseName = null
    )
    {
        if (builder.postgresResource is null)
        {
            // https://learn.microsoft.com/en-us/dotnet/aspire/database/postgresql-integration?tabs=dotnet-cli#add-postgresql-server-resource
            // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/app-host-overview#execution-context
            // consider each database name as a connection string name to connect to an existing server instance like `catalogsdb` as a connection string name
            return builder.applicationBuilder.AddConnectionString(nameOrConnectionStringName);
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);

        // use database name to create a connection string with `databaseNameOrConnectionStringName` new database name and using existing parent connection string in PostgresServerResource for creating a new connection resource for this database
        return builder.postgresResource.AddDatabase(name: nameOrConnectionStringName, databaseName: databaseName);
    }

    private static class PostgresDefaults
    {
        public const string ContainerImageName = "postgres";
        public const string ContainerImageTag = "latest";

        public const string DefaultResourceName = "postgres";
        public const string InitScriptTargetPath = "/docker-entrypoint-initdb.d/init-postgres.sql";

        public const int ContainerPort = 5432;
        public const int ProxyOrContainerHostPort = 5432;
    }
}
