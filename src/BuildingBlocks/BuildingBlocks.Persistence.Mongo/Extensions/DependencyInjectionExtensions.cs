using BuildingBlocks.Abstractions.Persistence.Mongo;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.HostApplicationBuilderExtensions;
using BuildingBlocks.Core.Extensions.ServiceCollectionExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;

namespace BuildingBlocks.Persistence.Mongo.Extensions;

public static class DependencyInjectionExtensions
{
    // use jbogard https://github.com/jbogard/MongoDB.Driver.Core.Extensions.DiagnosticSources package
    private const string ActivityNameSource = "MongoDB.Driver.Core.Extensions.DiagnosticSources";

    public static IHostApplicationBuilder AddMongoDbContext<TContext>(
        this IHostApplicationBuilder builder,
        string? connectionStringName = null,
        Action<MongoClientSettings>? configureClientSettings = null
    )
        where TContext : MongoDbContext, IMongoDbContext
    {
        builder.Services.AddValidationOptions<MongoOptions>(nameof(MongoOptions));

        var mongoOptions = builder.Configuration.BindOptions<MongoOptions>();

        // ConnectionString is not injected by aspire
        if (
            string.IsNullOrWhiteSpace(connectionStringName)
            || string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString(connectionStringName))
        )
        {
            var connectionString =
                mongoOptions.ConnectionString
                ?? throw new InvalidOperationException("`MongoOptions.ConnectionString` can't be null.");

            // https://github.com/dotnet/aspire/issues/10685
            builder.Services.AddSingleton<IMongoClient>(sp =>
                CreateMongoDBClient(configureClientSettings, connectionString, mongoOptions.DisableTracing, sp)
            );

            builder.AddMongoDatabase(connectionString);

            ConfigureInstrumentation(builder, mongoOptions);
        }
        else
        {
            // - EnvironmentVariablesConfigurationProvider is injected by aspire and use to read configuration values from environment variables with `ConnectionStrings:mongo-catalogsdb` key on configuration.
            // The configuration provider handles these conversions automatically, and `__ (double underscore)` becomes `:` for nested sections, so environment configuration reads its data from the ` ConnectionStrings__mongo-catalogsdb ` environment. all envs are available in `Environment.GetEnvironmentVariables()`.
            // - For setting none sensitive configuration, we can use Aspire named configuration `Aspire:Mongo:Driver:DisableHealthChecks` which is of type ConfigurationProvider and should be set in appsetting.json

            // https://learn.microsoft.com/en-us/dotnet/aspire/database/mongodb-integration?tabs=dotnet-cli#add-mongodb-client
            // ConnectionString injected by aspire
            builder.AddMongoDBClient(connectionStringName);
        }

        // Note: the serializers registrations and conventions should call just once whole of the application, otherwise we get an error

        // http://mongodb.github.io/mongo-csharp-driver/2.18/reference/bson/serialization/
        // http://mongodb.github.io/mongo-csharp-driver/2.18/reference/bson/guidserialization/guidrepresentationmode/guidrepresentationmode/
        // http://mongodb.github.io/mongo-csharp-driver/2.18/reference/bson/guidserialization/serializerchanges/guidserializerchanges/

        // https://stackoverflow.com/questions/21386347/how-do-i-detect-whether-a-mongodb-serializer-is-already-registered
        // https://stackoverflow.com/questions/16185262/what-is-new-way-of-setting-datetimeserializationoptions-defaults-in-mongodb-c-sh

        // we can write our own serializer register it with `RegisterSerializationProvider` and this serializer will work before default serializers.
        // BsonSerializer.RegisterSerializationProvider(new LocalDateTimeSerializationProvider());
        // Or
        BsonSerializer.RegisterSerializer(DateTimeSerializer.LocalInstance);
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.CSharpLegacy));

        RegisterConventions();

        builder.Services.AddScoped<TContext>();
        builder.Services.AddScoped<IMongoDbContext>(sp => sp.GetRequiredService<TContext>());

        builder.Services.AddTransient(typeof(IMongoRepository<,>), typeof(MongoRepository<,>));
        builder.Services.AddTransient(typeof(IMongoUnitOfWork<>), typeof(MongoUnitOfWork<>));

        return builder;
    }

    private static void ConfigureInstrumentation(IHostApplicationBuilder builder, MongoOptions mongoOptions)
    {
        if (!mongoOptions.DisableTracing)
        {
            builder.Services.AddOpenTelemetry().WithTracing(tracer => tracer.AddSource(ActivityNameSource));
        }

        if (!mongoOptions.DisableHealthChecks)
        {
            builder.TryAddHealthCheck(
                name: "MongoDB.Driver",
                healthCheckRegistration =>
                {
                    healthCheckRegistration.AddMongoDb(
                        clientFactory: sp => sp.GetRequiredService<IMongoClient>(),
                        name: "MongoDB.Driver",
                        tags: ["live"]
                    );
                }
            );
        }
    }

    private static IMongoClient CreateMongoDBClient(
        Action<MongoClientSettings>? configureClientSettings,
        string connectionString,
        bool disableTracing,
        IServiceProvider sp
    )
    {
        var clientSettings = MongoClientSettings.FromConnectionString(connectionString);

        if (!disableTracing)
        {
            clientSettings.ClusterConfigurator = cb => cb.Subscribe(new DiagnosticsActivityEventSubscriber());
        }

        configureClientSettings?.Invoke(clientSettings);

        clientSettings.LoggingSettings ??= new LoggingSettings(sp.GetService<ILoggerFactory>());

        return new MongoClient(connectionString);
    }

    private static void AddMongoDatabase(this IHostApplicationBuilder builder, string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentNullException(nameof(connectionString), "MongoDB connection string is required.");

        var mongoUrl = new MongoUrl(connectionString);

        if (string.IsNullOrWhiteSpace(mongoUrl.DatabaseName))
            throw new InvalidOperationException("MongoDB connection string must specify a database name.");

        builder.Services.AddSingleton<IMongoDatabase>(provider =>
            provider.GetRequiredService<IMongoClient>().GetDatabase(mongoUrl.DatabaseName)
        );
    }

    private static void RegisterConventions()
    {
        ConventionRegistry.Register(
            "conventions",
            new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreExtraElementsConvention(true),
                new EnumRepresentationConvention(BsonType.String),
                new IgnoreIfDefaultConvention(false),
                new ImmutablePocoConvention(),
            },
            _ => true
        );
    }
}
