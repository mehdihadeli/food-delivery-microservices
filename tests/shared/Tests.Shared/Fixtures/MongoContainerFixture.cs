using Ardalis.GuardClauses;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Tests.Shared.Helpers;
using MongoDB.Driver;

namespace Tests.Shared.Fixtures;

public class MongoContainerFixture : IAsyncLifetime
{
    private readonly MongoContainerOptions _mongoContainerOptions;
    public MongoDbTestcontainer Container { get; }

    public MongoContainerFixture()
    {
        var mongoContainerOptions = ConfigurationHelper.BindOptions<MongoContainerOptions>();
        Guard.Against.Null(mongoContainerOptions);
        _mongoContainerOptions = mongoContainerOptions;

        var postgresContainerBuilder = new TestcontainersBuilder<MongoDbTestcontainer>()
            .WithDatabase(new MongoDbTestcontainerConfiguration
            {
                Database = mongoContainerOptions.DatabaseName,
                Username = mongoContainerOptions.UserName,
                Password = mongoContainerOptions.Password,
            })
            .WithName(mongoContainerOptions.Name)
            .WithCleanUp(true)
            .WithImage(mongoContainerOptions.ImageName);

        Container = postgresContainerBuilder.Build();
    }

    public async Task ResetDbAsync(CancellationToken cancellationToken = default)
    {
        await DropDatabaseCollections(cancellationToken);
    }

    public async Task InitializeAsync()
    {
        await Container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await Container.StopAsync();
        await Container.DisposeAsync(); //important for the event to cleanup to be fired!
    }

    private async Task DropDatabaseCollections(CancellationToken cancellationToken)
    {
        //https://stackoverflow.com/questions/3366397/delete-everything-in-a-mongodb-database
        MongoClient dbClient = new MongoClient(Container.ConnectionString);
        var collections = await dbClient.GetDatabase(Container.Database)
            .ListCollectionsAsync(cancellationToken: cancellationToken);

        foreach (var collection in collections.ToList())
        {
            await dbClient.GetDatabase(Container.Database).DropCollectionAsync(collection["name"].AsString, cancellationToken);
        }
    }
    private  sealed class MongoContainerOptions
    {
        public string Name { get; set; } = "mongo_" + Guid.NewGuid();
        public string ImageName { get; set; } = "mongo:latest";
        public string DatabaseName { get; set; } = "test_db";
        public string UserName { get; set; } = "admin";
        public string Password { get; set; } = "admin";
    }
}
