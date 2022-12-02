using FluentAssertions;

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
        await _fixture.CleanupAsync();
    }

    public async Task InitializeAsync()
    {
        _fixture = new RabbitMQContainerFixture();
        await _fixture.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }
}
