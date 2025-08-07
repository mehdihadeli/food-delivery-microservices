using FluentAssertions;
using NSubstitute;
using Tests.Shared.XunitCategories;
using Xunit;
using Xunit.Sdk;

namespace Tests.Shared.Fixtures.Tests;

public class PostgresContainerFixtureTests : IAsyncLifetime
{
    private PostgresContainerFixture _fixture = default!;

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public async Task init_container()
    {
        _fixture.PostgresContainer.Should().NotBeNull();
        _fixture.PostgresContainer.GetConnectionString().Should().NotBeEmpty();
    }

    // [Fact]
    // [CategoryTrait(TestCategory.Unit)]
    // [CategoryTrait(TestCategory.Unit)]
    // public async Task reset_database()
    // {
    //     await _fixture.ResetDbAsync();
    // }

    public async ValueTask InitializeAsync()
    {
        var sink = Substitute.For<IMessageSink>();
        _fixture = new PostgresContainerFixture(sink);
        await _fixture.InitializeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }
}
