using System.Diagnostics.CodeAnalysis;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace BuildingBlocks.AspireIntegrations.Mongo;

[Experimental("ASPIREPROXYENDPOINTS001")]
public static class MongoBuilderExtensions
{
    /// <summary>
    /// Configures a MongoDB server as an Aspire resource with flexible port binding options.
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
    ///       Aspire generates a random host port for the container and uses the specified port (typically 27017) as the proxy port.
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
    ///       The container binds directly to the specified host port (typically 27017) with no proxy involved.
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
    /// When true and in publish mode, references an existing MongoDB instance instead of creating a container (default: false)
    /// </param>
    /// <param name="proxyEnabled">
    /// Enables Aspire's proxy for endpoint routing. When true, host ports are not exposed directly (default: true)
    /// </param>
    /// <param name="persistenceEnabled">
    /// Configures the container with persistent storage when true (default: false)
    /// </param>
    /// <param name="proxyOrContainerHostPort">
    /// Port for MongoDB (27017). Behavior depends on proxyEnabled:
    /// - When proxyEnabled: Proxy port (null for auto-assigned)
    /// - When !proxyEnabled: Host port (null for auto-assigned)
    /// </param>
    /// <param name="username">
    /// MongoDB root username as a ParameterResource (default: creates a parameter with value "admin")
    /// </param>
    /// <param name="password">
    /// MongoDB root password as a ParameterResource (default: creates a parameter with value "admin")
    /// </param>
    /// <returns>
    /// A tuple containing:
    /// - mongoResource: The MongoDB resource builder (null when useExistingServerInstance is true)
    /// - applicationBuilder: The distributed application builder for method chaining
    /// </returns>
    public static (
        IResourceBuilder<MongoDBServerResource>? mongoResource,
        IDistributedApplicationBuilder applicationBuilder
    ) AddAspireMongoDB(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string nameOrConnectionStringName,
        bool useExistingServerInstance = false,
        bool proxyEnabled = true,
        bool persistenceEnabled = false,
        int? proxyOrContainerHostPort = MongoDBDefaults.ProxyOrContainerHostPort,
        IResourceBuilder<ParameterResource>? username = null,
        IResourceBuilder<ParameterResource>? password = null
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(nameOrConnectionStringName);

        if (useExistingServerInstance && builder.ExecutionContext.IsPublishMode)
        {
            return (null, builder);
        }

        // https://learn.microsoft.com/en-us/dotnet/aspire/database/mongodb-integration
        var mongoDb = builder
            .AddMongoDB(
                name: nameOrConnectionStringName,
                port: proxyOrContainerHostPort,
                userName: username,
                password: password
            )
            .WithContainerName(nameOrConnectionStringName)
            .WithImage(MongoDBDefaults.ContainerImageName, MongoDBDefaults.ContainerImageTag)
            // modify the existing endpoint with endpointName
            .WithEndpoint(
                endpointName: MongoDBDefaults.PrimaryEndpointName,
                callback: endpoint =>
                {
                    endpoint.TargetPort = MongoDBDefaults.ContainerPort;
                    endpoint.IsProxied = proxyEnabled;
                    endpoint.Port = proxyOrContainerHostPort;
                    endpoint.IsExternal = false;
                }
            )
            .WithImagePullPolicy(ImagePullPolicy.Missing);

        if (builder.ExecutionContext.IsPublishMode || persistenceEnabled)
        {
            mongoDb.WithLifetime(ContainerLifetime.Persistent);
            mongoDb.WithDataVolume("mongodb_data");
        }

        // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/networking-overview#ports-and-proxies
        // https://github.com/dotnet/aspire/issues/1637
        // - aspire has a proxy for endpoints for 2 Reason:
        // 1. to avoid forwarding the connection until the underling endpoint is ready
        // 2. to route to multiple replicas of the underlying service
        //
        // - if proxy is disabled, this port will be the host port of the container, and if proxy is disabled by `WithEndpointProxySupport(false)`
        // this port will use by aspire proxy to route to a random generated host container port.
        mongoDb.WithEndpointProxySupport(proxyEnabled);

        return (mongoDb, builder);
    }

    /// <summary>
    /// Adds a MongoDB database connection resource to the distributed application using the provided builder tuple.
    /// If a MongoDB server resource is available, creates a new database on that server and registers its connection string using the specified name.
    /// If no server resource is provided, register the given name as a connection string for connecting to a pre-existing or external MongoDB server instance.
    /// </summary>
    /// <param name="builder">
    /// A tuple containing an optional MongoDB server resource and the distributed application builder.
    /// </param>
    /// <param name="nameOrConnectionStringName">
    /// The name of a mongo database resource (if creating one) or the key/name of an existing connection string for remote/external servers.
    /// </param>
    /// <param name="databaseName">database name</param>
    /// <returns>
    /// A resource builder for the configured MongoDB database connection, either newly created or from an existing connection string.
    /// </returns>
    public static IResourceBuilder<IResourceWithConnectionString> AddAspireMongoDatabase(
        this (
            IResourceBuilder<MongoDBServerResource>? mongoResource,
            IDistributedApplicationBuilder applicationBuilder
        ) builder,
        string nameOrConnectionStringName,
        string? databaseName = null
    )
    {
        if (builder.mongoResource is null)
        {
            // https://learn.microsoft.com/en-us/dotnet/aspire/database/mongodb-integration?tabs=dotnet-cli#add-mongodb-server-resource-and-database-resource
            // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/app-host-overview#execution-context
            // consider each database name as a connection string name to connect to an existing server instance like `catalogsdb` as a connection string name
            return builder.applicationBuilder.AddConnectionString(nameOrConnectionStringName);
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);

        // use database name to create a connection string with `databaseNameOrConnectionStringName` new database name and using existing parent connection string in PostgresServerResource for creating a new connection resource for this database
        return builder.mongoResource.AddDatabase(name: nameOrConnectionStringName, databaseName: databaseName);
    }

    private static class MongoDBDefaults
    {
        public const string ContainerImageName = "mongo";
        public const string ContainerImageTag = "latest";

        public const string DefaultResourceName = "mongo";

        public const string PrimaryEndpointName = "tcp";

        public const int ContainerPort = 27017;
        public const int ProxyOrContainerHostPort = 27017;
    }
}
