using FluentAssertions;
using Mongo2Go;

namespace Tests.Shared.Fixtures.Tests;

public class Mongo2GoFixtureTests : IAsyncLifetime
{
    private Mongo2GoFixture _fixture = default!;

    [Fact]
    public async Task init_fixture()
    {
        _fixture.MongoDbRunner.Should().NotBeNull();
        _fixture.MongoDbRunner.ConnectionString.Should().NotBeEmpty();
    }

    [Fact]
    public async Task reset_database()
    {
       await _fixture.ResetDbAsync();
       _fixture.MongoDbRunner.State.Should().Be(State.Running);
    }

    public async Task InitializeAsync()
    {
        _fixture = new Mongo2GoFixture();
        await _fixture.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }
}
