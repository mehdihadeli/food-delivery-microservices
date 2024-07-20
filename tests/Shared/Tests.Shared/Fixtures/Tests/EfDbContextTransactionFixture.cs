using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Messaging.Persistence.Postgres.MessagePersistence;
using FluentAssertions;
using NSubstitute;
using Tests.Shared.XunitCategories;

namespace Tests.Shared.Fixtures.Tests;

public class EfDbContextTransactionFixture : IAsyncLifetime
{
    private EfDbContextTransactionFixture<MessagePersistenceDbContext> _fixture = default!;

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public async Task init_container()
    {
        _fixture.Container.Should().NotBeNull();
        _fixture.Container.GetConnectionString().Should().NotBeEmpty();
        _fixture.DbContext.Should().NotBeNull();
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
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
        var sink = Substitute.For<IMessageSink>();
        _fixture = new EfDbContextTransactionFixture<MessagePersistenceDbContext>(sink);
        await _fixture.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }
}
