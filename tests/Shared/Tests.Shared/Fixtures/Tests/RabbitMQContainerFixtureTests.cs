using FluentAssertions;
using NSubstitute;
using Tests.Shared.XunitCategories;
using Xunit;
using Xunit.Sdk;

namespace Tests.Shared.Fixtures.Tests;

public class RabbitMQContainerFixtureTests : IAsyncLifetime
{
    private RabbitMQContainerFixture _fixture = default!;

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public async Task init_container()
    {
        _fixture.Container.Should().NotBeNull();
        _fixture.Container.GetConnectionString().Should().NotBeEmpty();
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public async Task cleanup_messaging()
    {
        await _fixture.CleanupQueuesAsync();
    }

    public async ValueTask InitializeAsync()
    {
        var sink = Substitute.For<IMessageSink>();
        _fixture = new RabbitMQContainerFixture(sink);
        await _fixture.InitializeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }
}
