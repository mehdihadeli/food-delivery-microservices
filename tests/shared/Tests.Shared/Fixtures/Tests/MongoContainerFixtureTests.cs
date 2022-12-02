using FluentAssertions;
using Humanizer;
using MongoDB.Driver;

namespace Tests.Shared.Fixtures.Tests;

public class MongoContainerFixtureTests : IAsyncLifetime
{
    private MongoContainerFixture _fixture = default!;

    [Fact]
    public async Task init_container()
    {
        _fixture.Container.Should().NotBeNull();
        _fixture.Container.ConnectionString.Should().NotBeEmpty();
    }

    [Fact]
    public async Task reset_database()
    {
        MongoClient dbClient = new MongoClient(_fixture.Container.ConnectionString);
        await dbClient.GetDatabase(_fixture.Container.Database).CreateCollectionAsync(nameof(TestDocument).Underscore());
        var testDoc = dbClient.GetDatabase(_fixture.Container.Database)
            .GetCollection<TestDocument>(nameof(TestDocument).Underscore());
        await testDoc.InsertOneAsync(new TestDocument() {Name = "test data"});

        await _fixture.ResetDbAsync();

        var collections = await dbClient.GetDatabase(_fixture.Container.Database)
            .ListCollectionsAsync();

        collections.ToList().Should().BeEmpty();
    }

    public async Task InitializeAsync()
    {
        _fixture = new MongoContainerFixture();
        await _fixture.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }

    internal class TestDocument
    {
        public string Name { get; set; }
    }
}
