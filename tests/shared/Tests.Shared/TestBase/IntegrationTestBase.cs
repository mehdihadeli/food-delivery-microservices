using BuildingBlocks.Persistence.Mongo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tests.Shared.Fixtures;
using Tests.Shared.XunitCategories;

namespace Tests.Shared.TestBase;

//https://bartwullems.blogspot.com/2019/09/xunit-async-lifetime.html
//https://www.danclarke.com/cleaner-tests-with-iasynclifetime
//https://xunit.net/docs/shared-context
public abstract class IntegrationTest<TEntryPoint> : IAsyncLifetime
    where TEntryPoint : class
{
    protected CancellationToken CancellationToken => CancellationTokenSource.Token;
    protected CancellationTokenSource CancellationTokenSource { get; }
    protected int Timeout => 180;
    protected IServiceScope Scope { get; }
    protected SharedFixture<TEntryPoint> SharedFixture { get; }
    protected IntegrationTest(
        SharedFixture<TEntryPoint> sharedFixture,
        ITestOutputHelper outputHelper)
    {
        SharedFixture = sharedFixture;
        SharedFixture.SetOutputHelper(outputHelper);

        CancellationTokenSource = new(TimeSpan.FromSeconds(Timeout));
        CancellationToken.ThrowIfCancellationRequested();

        SharedFixture.WithConfigureAppConfigurations((context, configurationBuilder) =>
        {
            RegisterTestAppConfigurations(configurationBuilder, context.Configuration, context.HostingEnvironment);
        });

        SharedFixture.ConfigureTestServices(RegisterTestConfigureServices);

        // Build Service Provider here
        Scope = SharedFixture.ServiceProvider.CreateScope();
    }

    // we use IAsyncLifetime in xunit instead of constructor when we have async operation
    public virtual async Task InitializeAsync()
    {
    }

    public virtual async Task DisposeAsync()
    {
        // it is better messages delete first
        await SharedFixture.CleanupMessaging(CancellationToken);
        await SharedFixture.ResetDatabasesAsync(CancellationToken);

        CancellationTokenSource.Cancel();

        Scope.Dispose();
    }

    protected virtual void RegisterTestConfigureServices(IServiceCollection services)
    {
    }

    protected virtual void RegisterTestAppConfigurations(
        IConfigurationBuilder builder,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
    }
}

public abstract class IntegrationTestBase<TEntryPoint, TContext> : IntegrationTest<TEntryPoint>
    where TEntryPoint : class
    where TContext : DbContext
{
    protected IntegrationTestBase(
        SharedFixtureWithEfCore<TEntryPoint, TContext> sharedFixture, ITestOutputHelper outputHelper)
        : base(sharedFixture, outputHelper)
    {
        SharedFixture = sharedFixture;
    }

    public new SharedFixtureWithEfCore<TEntryPoint, TContext> SharedFixture { get; }
}

public abstract class
    IntegrationTestBase<TEntryPoint, TWContext, TRContext> : IntegrationTest<TEntryPoint>
    where TEntryPoint : class
    where TWContext : DbContext
    where TRContext : MongoDbContext
{
    protected IntegrationTestBase(
        SharedFixtureWithEfCoreAndMongo<TEntryPoint, TWContext, TRContext> sharedFixture,
        ITestOutputHelper outputHelper) : base(sharedFixture, outputHelper)
    {
        SharedFixture = sharedFixture;
    }

    public new SharedFixtureWithEfCoreAndMongo<TEntryPoint, TWContext, TRContext> SharedFixture { get; }
}
