using Mongo2Go;

namespace Tests.Shared.Fixtures;

public class Mongo2GoFixture : IAsyncLifetime
{
    public MongoDbRunner MongoDbRunner { get; set; } = default!;

    public Task InitializeAsync()
    {
        MongoDbRunner = MongoDbRunner.Start();

        return Task.CompletedTask;
    }

    public async Task ResetDbAsync(CancellationToken cancellationToken = default)
    {
        MongoDbRunner.Dispose();
        MongoDbRunner = MongoDbRunner.Start();
    }


    public Task DisposeAsync()
    {
        MongoDbRunner.Dispose();
        MongoDbRunner = null;
        return Task.CompletedTask;
    }
}
