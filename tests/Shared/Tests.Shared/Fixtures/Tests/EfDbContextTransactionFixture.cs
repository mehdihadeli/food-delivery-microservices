using BuildingBlocks.Abstractions.Messages.MessagePersistence;
using BuildingBlocks.Messaging.Persistence.Postgres.MessagePersistence;
using FluentAssertions;
using NSubstitute;
using Tests.Shared.XunitCategories;
using Xunit;
using Xunit.Sdk;

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
        _fixture.DbContext.PersistMessages.Add(
            new PersistMessage(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "ss",
                "ss",
                MessageDeliveryType.Inbox,
                MessageStatus.Stored
            )
        );
        await _fixture.DbContext.SaveChangesAsync();

        _fixture.DbContext.PersistMessages.Count().Should().Be(1);
        await _fixture.ResetAsync();
        _fixture.DbContext.PersistMessages.Count().Should().Be(0);
    }

    public async ValueTask InitializeAsync()
    {
        var sink = Substitute.For<IMessageSink>();
        _fixture = new EfDbContextTransactionFixture<MessagePersistenceDbContext>(sink);
        await _fixture.InitializeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }
}
