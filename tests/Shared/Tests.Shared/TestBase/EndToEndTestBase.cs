using BuildingBlocks.Persistence.Mongo;
using Microsoft.EntityFrameworkCore;
using Tests.Shared.Fixtures;

namespace Tests.Shared.TestBase;

public class EndToEndTestTest<TEntryPoint>(SharedFixture<TEntryPoint> sharedFixture, ITestOutputHelper outputHelper)
    : IntegrationTest<TEntryPoint>(sharedFixture, outputHelper)
    where TEntryPoint : class;

public abstract class EndToEndTestTestBase<TEntryPoint, TContext>(
    SharedFixtureWithEfCore<TEntryPoint, TContext> sharedFixture,
    ITestOutputHelper outputHelper
) : EndToEndTestTest<TEntryPoint>(sharedFixture, outputHelper)
    where TEntryPoint : class
    where TContext : DbContext
{
    public new SharedFixtureWithEfCore<TEntryPoint, TContext> SharedFixture { get; } = sharedFixture;
}

public abstract class EndToEndTestTestBase<TEntryPoint, TWContext, TRContext>(
    SharedFixtureWithEfCoreAndMongo<TEntryPoint, TWContext, TRContext> sharedFixture,
    ITestOutputHelper outputHelper
) : EndToEndTestTest<TEntryPoint>(sharedFixture, outputHelper)
    where TEntryPoint : class
    where TWContext : DbContext
    where TRContext : MongoDbContext
{
    public new SharedFixtureWithEfCoreAndMongo<TEntryPoint, TWContext, TRContext> SharedFixture { get; } =
        sharedFixture;
}
