using Mongo2Go;
using Xunit.Sdk;

namespace Tests.Shared.Fixtures;

public class Mongo2GoFixture : IAsyncLifetime
{
    private readonly IMessageSink _messageSink;

    public Mongo2GoFixture(IMessageSink messageSink)
    {
        _messageSink = messageSink;
    }

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
        _messageSink.OnMessage(
            new DiagnosticMessage($"Mongo fixture started on connection string: {MongoDbRunner.ConnectionString}...")
        );
    }

    public Task DisposeAsync()
    {
        MongoDbRunner.Dispose();
        MongoDbRunner = null;
        _messageSink.OnMessage(new DiagnosticMessage("Mongo fixture stopped."));
        return Task.CompletedTask;
    }
}
