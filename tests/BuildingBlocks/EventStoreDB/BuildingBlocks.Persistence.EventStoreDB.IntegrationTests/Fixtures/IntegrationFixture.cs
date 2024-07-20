using BuildingBlocks.Abstractions.Persistence.EventStore;
using BuildingBlocks.Core.Registrations;
using BuildingBlocks.Persistence.EventStoreDB.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tests.Shared.Helpers;

namespace BuildingBlocks.Persistence.EventStoreDB.IntegrationTests.Fixtures;

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
