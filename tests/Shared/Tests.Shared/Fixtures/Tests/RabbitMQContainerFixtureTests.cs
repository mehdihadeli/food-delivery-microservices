using FluentAssertions;
using NSubstitute;

namespace Tests.Shared.Fixtures.Tests;

public class RabbitMQContainerFixtureTests : IAsyncLifetime
{
    private RabbitMQContainerFixture _fixture = default!;

    [Fact]
    public async Task init_container()
    {
        _fixture.Container.Should().NotBeNull();
        _fixture.Container.ConnectionString.Should().NotBeEmpty();
    }

    [Fact]
    public async Task cleanup_messaging()
    {
        await _fixture.CleanupQueuesAsync();
    }

    public async Task InitializeAsync()
    {
        var sink = Substitute.For<IMessageSink>();
        _fixture = new RabbitMQContainerFixture(sink);
        await _fixture.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }
}
