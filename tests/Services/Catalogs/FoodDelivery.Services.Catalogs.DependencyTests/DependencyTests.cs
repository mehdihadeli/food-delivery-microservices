using BuildingBlocks.Caching;
using BuildingBlocks.Core.Extensions.ServiceCollectionExtensions;
using BuildingBlocks.Core.Messages.MessagePersistence;
using BuildingBlocks.Core.Web;
using BuildingBlocks.Integration.MassTransit;
using BuildingBlocks.Persistence.EfCore.Postgres;
using BuildingBlocks.Persistence.Mongo;
using FoodDelivery.Services.Catalogs.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tests.Shared.Factory;

namespace FoodDelivery.Services.Catalogs.DependencyTests;

public class DependencyTests
{
    [Fact]
    public void validate_service_dependencies()
    {
        var factory = new CustomWebApplicationFactory<CatalogsApiMetadata>()
            .WithTestConfigureServices(services =>
            {
                services.TryAddTransient<IServiceCollection>(_ => services);
            })
            .WithEnvironment(Environments.DependencyTest)
            .AddOverrideEnvKeyValues(keyValues =>
            {
                keyValues.Add(
                    $"{nameof(PostgresOptions)}__{nameof(PostgresOptions.ConnectionString)}",
                    "Server=localhost;Port=5432;Database=catalogs;User Id=postgres;Password=postgres;Include Error Detail=true"
                );
                keyValues.Add(
                    $"{nameof(MessagePersistenceOptions)}__{nameof(PostgresOptions.ConnectionString)}",
                    "Server=localhost;Port=5432;Database=catalogs;User Id=postgres;Password=postgres;Include Error Detail=true"
                );
                keyValues.Add(
                    $"{nameof(MongoOptions)}__{nameof(MongoOptions.ConnectionString)}",
                    "mongodb://admin:admin@localhost:27017/catalogs?authSource=admin&authMechanism=SCRAM-SHA-256"
                );
                keyValues.Add(
                    $"{nameof(MasstransitOptions)}__{nameof(MasstransitOptions.RabbitMQConnectionString)}",
                    "amqp://guest:guest@localhost:5672"
                );
                keyValues.Add(
                    $"{nameof(CacheOptions)}__{nameof(RedisDistributedCacheOptions)}__{nameof(RedisDistributedCacheOptions.ConnectionString)}",
                    "http://localhost:6379"
                );
            });

        using var scope = factory.Services.CreateScope();
        var sp = scope.ServiceProvider;
        var services = sp.GetRequiredService<IServiceCollection>();
        sp.ValidateDependencies(services, typeof(CatalogsApiMetadata).Assembly, typeof(CatalogsMetadata).Assembly);
    }
}
