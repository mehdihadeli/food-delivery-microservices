using System.Reflection;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Abstractions.Persistence.Mongo;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace BuildingBlocks.Persistence.Mongo;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongoDbContext<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<MongoOptions>? configurator = null,
        params Assembly[] assembliesToScan
    )
        where TContext : MongoDbContext, IMongoDbContext
    {
        services.AddValidatedOptions<MongoOptions>(nameof(MongoOptions));

        var options = configuration.BindOptions<MongoOptions>();
        configurator?.Invoke(options);

        // add option to the dependency injection
        services.AddValidationOptions<MongoOptions>(opt => configurator?.Invoke(opt));

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

        services.TryAddScoped(typeof(TContext));
        services.TryAddScoped<IMongoDbContext>(sp => sp.GetRequiredService<TContext>());

        services.AddMongoRepositories(assembliesToScan);
        services.AddMongoUnitOfWork(assembliesToScan);

        return services;
    }

    private static IServiceCollection AddMongoRepositories(
        this IServiceCollection services,
        params Assembly[] assembliesToScan
    )
    {
        var scanAssemblies = assembliesToScan.Any() ? assembliesToScan : new[] { Assembly.GetCallingAssembly() };
        services.Scan(
            scan =>
                scan.FromAssemblies(scanAssemblies)
                    .AddClasses(classes => classes.AssignableTo(typeof(IRepository<,>)), false)
                    .AsImplementedInterfaces()
                    .AsSelf()
                    .WithTransientLifetime()
        );

        return services;
    }

    private static IServiceCollection AddMongoUnitOfWork(
        this IServiceCollection services,
        params Assembly[] assembliesToScan
    )
    {
        var scanAssemblies = assembliesToScan.Any() ? assembliesToScan : new[] { Assembly.GetCallingAssembly() };
        services.Scan(
            scan =>
                scan.FromAssemblies(scanAssemblies)
                    .AddClasses(classes => classes.AssignableTo(typeof(IMongoUnitOfWork<>)), false)
                    .AsImplementedInterfaces()
                    .AsSelf()
                    .WithTransientLifetime()
        );

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
