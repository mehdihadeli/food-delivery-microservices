using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Squirrel.Abstractions.Persistence.EventStore;
using Squirrel.Core.Registrations;
using Squirrel.Persistence.EventStoreDB.Extensions;
using Tests.Shared.Helpers;

namespace Squirrel.Persistence.EventStoreDB.IntegrationTests.Fixtures;

public class IntegrationFixture : IAsyncLifetime
{
    private readonly ServiceProvider _provider;

    public IntegrationFixture()
    {
        var services = new ServiceCollection();
        var configuration = ConfigurationHelper.BuildConfiguration();

        services.AddCore();
        services.AddLogging(builder =>
        {
            builder.AddXUnit();
        });
        services.AddCqrs();
        services.AddEventStoreDb(configuration);
        services.AddHttpContextAccessor();

        _provider = services.BuildServiceProvider();
    }

    public IEventStore EventStore => _provider.GetRequiredService<IEventStore>();

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return Task.CompletedTask;
    }
}
