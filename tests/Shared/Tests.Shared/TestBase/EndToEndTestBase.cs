using BuildingBlocks.Persistence.Mongo;
using Microsoft.EntityFrameworkCore;
using Tests.Shared.TestBase;
using Tests.Shared.XunitCategories;

namespace Tests.Shared.Fixtures;

public class EndToEndTestTest<TEntryPoint> : IntegrationTest<TEntryPoint>
    where TEntryPoint : class
{
    public EndToEndTestTest(SharedFixture<TEntryPoint> sharedFixture, ITestOutputHelper outputHelper)
        : base(sharedFixture, outputHelper) { }
}

public abstract class EndToEndTestTestBase<TEntryPoint, TContext> : EndToEndTestTest<TEntryPoint>
    where TEntryPoint : class
    where TContext : DbContext
{
    protected EndToEndTestTestBase(
        SharedFixtureWithEfCore<TEntryPoint, TContext> sharedFixture,
        ITestOutputHelper outputHelper
    )
        : base(sharedFixture, outputHelper)
    {
        SharedFixture = sharedFixture;
    }

    public new SharedFixtureWithEfCore<TEntryPoint, TContext> SharedFixture { get; }
}

public abstract class EndToEndTestTestBase<TEntryPoint, TWContext, TRContext> : EndToEndTestTest<TEntryPoint>
    where TEntryPoint : class
    where TWContext : DbContext
    where TRContext : MongoDbContext
{
    protected EndToEndTestTestBase(
        SharedFixtureWithEfCoreAndMongo<TEntryPoint, TWContext, TRContext> sharedFixture,
        ITestOutputHelper outputHelper
    )
        : base(sharedFixture, outputHelper)
    {
        SharedFixture = sharedFixture;
    }

    public new SharedFixtureWithEfCoreAndMongo<TEntryPoint, TWContext, TRContext> SharedFixture { get; }
}
