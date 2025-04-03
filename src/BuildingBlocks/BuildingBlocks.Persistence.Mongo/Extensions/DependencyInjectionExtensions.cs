using BuildingBlocks.Abstractions.Persistence.Mongo;
using BuildingBlocks.Core.Extensions.ServiceCollectionExtensions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace BuildingBlocks.Persistence.Mongo.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddMongoDbContext<TContext>(this IServiceCollection services)
        where TContext : MongoDbContext, IMongoDbContext
    {
        services.AddValidatedOptions<MongoOptions>(nameof(MongoOptions));

        services.AddSingleton<IMongoClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<MongoOptions>>();
            return new MongoClient(options.Value.ConnectionString);
        });

        services.AddSingleton<IMongoDatabase>(sp =>
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

        services.AddScoped(typeof(TContext));
        services.AddScoped<IMongoDbContext>(sp => sp.GetRequiredService<TContext>());

        services.AddTransient(typeof(IMongoRepository<,>), typeof(MongoRepository<,>));
        services.AddTransient(typeof(IMongoUnitOfWork<>), typeof(MongoUnitOfWork<>));

        return services;
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
