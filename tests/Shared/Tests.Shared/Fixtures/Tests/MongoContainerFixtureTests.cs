using FluentAssertions;
using Humanizer;
using MongoDB.Driver;
using NSubstitute;
using Tests.Shared.XunitCategories;
using Xunit;
using Xunit.Sdk;

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
            .CreateCollectionAsync(
                nameof(TestDocument).Underscore(),
                cancellationToken: TestContext.Current.CancellationToken
            );
        var testDoc = dbClient
            .GetDatabase(_fixture.MongoContainerOptions.DatabaseName)
            .GetCollection<TestDocument>(nameof(TestDocument).Underscore());
        await testDoc.InsertOneAsync(
            new TestDocument { Name = "test data" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        await _fixture.ResetDbAsync(TestContext.Current.CancellationToken);

        var collections = await dbClient
            .GetDatabase(_fixture.MongoContainerOptions.DatabaseName)
            .ListCollectionsAsync(cancellationToken: TestContext.Current.CancellationToken);

        collections.ToList(cancellationToken: TestContext.Current.CancellationToken).Should().BeEmpty();
    }

    public async ValueTask InitializeAsync()
    {
        var sink = Substitute.For<IMessageSink>();
        _fixture = new MongoContainerFixture(sink);
        await _fixture.InitializeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }

    internal class TestDocument
    {
        public string Name { get; set; }
    }
}
