using BuildingBlocks.Abstractions.Persistence.Mongo;
using BuildingBlocks.Core.Extensions.ServiceCollectionExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace BuildingBlocks.Persistence.Mongo.Extensions;

public static class DependencyInjectionExtensions
{
    public static IHostApplicationBuilder AddMongoDbContext<TContext>(this IHostApplicationBuilder builder)
        where TContext : MongoDbContext, IMongoDbContext
    {
        builder.Services.AddValidationOptions<MongoOptions>(nameof(MongoOptions));

        builder.Services.AddSingleton<IMongoClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<MongoOptions>>();
            return new MongoClient(options.Value.ConnectionString);
        });

        builder.Services.AddSingleton<IMongoDatabase>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<MongoOptions>>();
            var client = sp.GetRequiredService<IMongoClient>();

            return client.GetDatabase(options.Value.DatabaseName);
        });

        // Note: the serializers registrations and conventions should call just once whole of the application otherwise we get error

        // http://mongodb.github.io/mongo-csharp-driver/2.18/reference/bson/serialization/
        // http://mongodb.github.io/mongo-csharp-driver/2.18/reference/bson/guidserialization/guidrepresentationmode/guidrepresentationmode/
        // http://mongodb.github.io/mongo-csharp-driver/2.18/reference/bson/guidserialization/serializerchanges/guidserializerchanges/

        // https://stackoverflow.com/questions/21386347/how-do-i-detect-whether-a-mongodb-serializer-is-already-registered
        // https://stackoverflow.com/questions/16185262/what-is-new-way-of-setting-datetimeserializationoptions-defaults-in-mongodb-c-sh

        // we can write our own serializer register it with `RegisterSerializationProvider` and this serializer will work before default serializers.
        // BsonSerializer.RegisterSerializationProvider(new LocalDateTimeSerializationProvider());
        // Or
        BsonSerializer.RegisterSerializer(typeof(DateTime), DateTimeSerializer.LocalInstance);
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.CSharpLegacy));

        RegisterConventions();

        builder.Services.AddScoped(typeof(TContext));
        builder.Services.AddScoped<IMongoDbContext>(sp => sp.GetRequiredService<TContext>());

        builder.Services.AddTransient(typeof(IMongoRepository<,>), typeof(MongoRepository<,>));
        builder.Services.AddTransient(typeof(IMongoUnitOfWork<>), typeof(MongoUnitOfWork<>));

        builder
            .Services.AddHealthChecks()
            .AddMongoDb(
                dbFactory: sp => sp.GetRequiredService<IMongoDatabase>(),
                name: "Mongo-Check",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["live"],
                timeout: TimeSpan.FromSeconds(5)
            );

        return builder;
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
