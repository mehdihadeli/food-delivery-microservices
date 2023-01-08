using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Messaging.Persistence.Postgres.MessagePersistence;
using FluentAssertions;

namespace Tests.Shared.Fixtures.Tests;

public class EfDbContextTransactionFixture : IAsyncLifetime
{
    private EfDbContextTransactionFixture<MessagePersistenceDbContext> _fixture = default!;

    [Fact]
    public async Task init_container()
    {
        _fixture.Container.Should().NotBeNull();
        _fixture.Container.ConnectionString.Should().NotBeEmpty();
        _fixture.DbContext.Should().NotBeNull();
    }

    [Fact]
    public async Task reset_database()
    {
        _fixture.DbContext.StoreMessages.Add(new StoreMessage(Guid.NewGuid(), "ss", "ss", MessageDeliveryType.Inbox));
        await _fixture.DbContext.SaveChangesAsync();

        _fixture.DbContext.StoreMessages.Count().Should().Be(1);
        await _fixture.ResetAsync();
        _fixture.DbContext.StoreMessages.Count().Should().Be(0);
    }

    public async Task InitializeAsync()
    {
        _fixture = new EfDbContextTransactionFixture<MessagePersistenceDbContext>();
        await _fixture.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }
}
