using Ardalis.GuardClauses;
using Tests.Shared.Helpers;
using MongoDB.Driver;
using Testcontainers.MongoDb;
using Xunit.Sdk;

namespace Tests.Shared.Fixtures;

public class MongoContainerFixture : IAsyncLifetime
{
    private readonly IMessageSink _messageSink;

    public MongoContainerOptions MongoContainerOptions { get; }
    public MongoDbContainer Container { get; }
    public int HostPort => Container.GetMappedPublicPort(MongoDbBuilder.MongoDbPort);
    public int TcpContainerPort => MongoDbBuilder.MongoDbPort;

    public MongoContainerFixture(IMessageSink messageSink)
    {
        _messageSink = messageSink;
        var mongoContainerOptions = ConfigurationHelper.BindOptions<MongoContainerOptions>();
        Guard.Against.Null(mongoContainerOptions);
        MongoContainerOptions = mongoContainerOptions;

        var postgresContainerBuilder = new MongoDbBuilder()
            .WithUsername(mongoContainerOptions.UserName)
            .WithPassword(mongoContainerOptions.Password)
            .WithName(mongoContainerOptions.Name)
            .WithCleanUp(true)
            // https://github.com/testcontainers/testcontainers-dotnet/issues/734
            // testcontainers has a problem with using mongo:latest version for now we use testcontainer default image
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
        _messageSink.OnMessage(
            new DiagnosticMessage(
                $"Mongo fixture started on Host port {HostPort} and container tcp port {TcpContainerPort}..."
            )
        );
    }

    public async Task DisposeAsync()
    {
        await Container.StopAsync();
        await Container.DisposeAsync(); //important for the event to cleanup to be fired!
        _messageSink.OnMessage(new DiagnosticMessage("Mongo fixture stopped."));
    }

    private async Task DropDatabaseCollections(CancellationToken cancellationToken)
    {
        //https://stackoverflow.com/questions/3366397/delete-everything-in-a-mongodb-database
        MongoClient dbClient = new MongoClient(Container.GetConnectionString());

        //// Drop database completely in each run or drop only the collections in exisitng database
        //await dbClient.DropDatabaseAsync(Container.Database, cancellationToken);

        var collections = await dbClient
            .GetDatabase(MongoContainerOptions.DatabaseName)
            .ListCollectionsAsync(cancellationToken: cancellationToken);

        foreach (var collection in collections.ToList())
        {
            await dbClient
                .GetDatabase(MongoContainerOptions.DatabaseName)
                .DropCollectionAsync(collection["name"].AsString, cancellationToken);
        }
    }
}

public sealed class MongoContainerOptions
{
    public string Name { get; set; } = "mongo_" + Guid.NewGuid();
    public string ImageName { get; set; } = "mongo:latest";
    public string DatabaseName { get; set; } = "test_db";
    public string UserName { get; set; } = "admin";
    public string Password { get; set; } = "admin";
}
