using FluentAssertions;

namespace Tests.Shared.Fixtures.Tests;

public class PostgresContainerFixtureTests : IAsyncLifetime
{
    private PostgresContainerFixture _fixture = default!;

    [Fact]
    public async Task init_container()
    {
        _fixture.Container.Should().NotBeNull();
        _fixture.Container.ConnectionString.Should().NotBeEmpty();
    }

    [Fact]
    public async Task reset_database()
    {
        await _fixture.ResetDbAsync();
    }

    public async Task InitializeAsync()
    {
        _fixture = new PostgresContainerFixture();
        await _fixture.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }
}
