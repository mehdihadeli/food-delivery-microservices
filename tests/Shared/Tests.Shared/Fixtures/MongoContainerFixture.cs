using BuildingBlocks.Core.Extensions;
using MongoDB.Driver;
using Testcontainers.MongoDb;
using Tests.Shared.Helpers;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace Tests.Shared.Fixtures;

public class MongoContainerFixture : IAsyncLifetime
{
    private readonly IMessageSink _messageSink;
    private string? _connectionString;

    public MongoContainerOptions MongoContainerOptions { get; }
    public MongoDbContainer Container { get; }
    public int HostPort => Container.GetMappedPublicPort(MongoDbBuilder.MongoDbPort);
    public int TcpContainerPort => MongoDbBuilder.MongoDbPort;

    // like something generates by mongo aspire connection for a database
    public string ConnectionString =>
        _connectionString ??=
            $"{
                Container.GetConnectionString().Replace("?directConnection=true", "", StringComparison.InvariantCulture)
            }{
                MongoContainerOptions.DatabaseName
            }?authSource=admin&authMechanism=SCRAM-SHA-256";

    public MongoContainerFixture(IMessageSink messageSink)
    {
        _messageSink = messageSink;
        var mongoContainerOptions = ConfigurationHelper.BindOptions<MongoContainerOptions>();
        mongoContainerOptions.NotBeNull();
        MongoContainerOptions = mongoContainerOptions;

        var mongoDbBuilder = new MongoDbBuilder()
            .WithName(mongoContainerOptions.Name)
            .WithCleanUp(true)
            // https://github.com/testcontainers/testcontainers-dotnet/issues/734
            // testcontainers has a problem with using mongo:latest version for now we use testcontainer default image
            .WithImage(mongoContainerOptions.ImageName);

        Container = mongoDbBuilder.Build();
    }

    public async Task ResetDbAsync(CancellationToken cancellationToken = default)
    {
        await DropDatabaseCollections(cancellationToken);
    }

    public async ValueTask InitializeAsync()
    {
        await Container.StartAsync();
        _messageSink.OnMessage(
            new DiagnosticMessage(
                $"Mongo fixture started on Host port {HostPort} and container tcp port {TcpContainerPort}..."
            )
        );
    }

    public async ValueTask DisposeAsync()
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
}
