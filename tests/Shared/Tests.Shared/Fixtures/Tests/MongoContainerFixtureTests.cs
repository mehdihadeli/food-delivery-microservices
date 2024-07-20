using FluentAssertions;
using Humanizer;
using MongoDB.Driver;
using NSubstitute;
using Tests.Shared.XunitCategories;

namespace Tests.Shared.Fixtures.Tests;

public class MongoContainerFixtureTests : IAsyncLifetime
{
    private MongoContainerFixture _fixture = default!;

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public async Task init_container()
    {
        _fixture.Container.Should().NotBeNull();
        _fixture.Container.GetConnectionString().Should().NotBeEmpty();
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public async Task reset_database()
    {
        MongoClient dbClient = new MongoClient(_fixture.Container.GetConnectionString());
        await dbClient
            .GetDatabase(_fixture.MongoContainerOptions.DatabaseName)
            .CreateCollectionAsync(nameof(TestDocument).Underscore());
        var testDoc = dbClient
            .GetDatabase(_fixture.MongoContainerOptions.DatabaseName)
            .GetCollection<TestDocument>(nameof(TestDocument).Underscore());
        await testDoc.InsertOneAsync(new TestDocument() { Name = "test data" });

        await _fixture.ResetDbAsync();

        var collections = await dbClient
            .GetDatabase(_fixture.MongoContainerOptions.DatabaseName)
            .ListCollectionsAsync();

        collections.ToList().Should().BeEmpty();
    }

    public async Task InitializeAsync()
    {
        var sink = Substitute.For<IMessageSink>();
        _fixture = new MongoContainerFixture(sink);
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
