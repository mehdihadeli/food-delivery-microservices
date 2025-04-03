using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Persistence;
using BuildingBlocks.Persistence.Mongo;
using Microsoft.AspNetCore.HeaderPropagation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Tests.Shared.Fixtures;

namespace Tests.Shared.TestBase;

//https://bartwullems.blogspot.com/2019/09/xunit-async-lifetime.html
//https://www.danclarke.com/cleaner-tests-with-iasynclifetime
//https://xunit.net/docs/shared-context
public abstract class IntegrationTest<TEntryPoint> : XunitContextBase, IAsyncLifetime
    where TEntryPoint : class
{
    private IServiceScope? _serviceScope;
    private TestWorkersRunner _testWorkersRunner = default!;

    protected CancellationToken CancellationToken => CancellationTokenSource.Token;
    protected CancellationTokenSource CancellationTokenSource { get; }
    protected int Timeout => 180;

    // Build Service Provider here
    protected IServiceScope Scope => _serviceScope ??= SharedFixture.ServiceProvider.CreateScope();
    protected SharedFixture<TEntryPoint> SharedFixture { get; }

    protected IntegrationTest(SharedFixture<TEntryPoint> sharedFixture, ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        SharedFixture = sharedFixture;
        SharedFixture.SetOutputHelper(outputHelper);

        CancellationTokenSource = new(TimeSpan.FromSeconds(Timeout));
        CancellationToken.ThrowIfCancellationRequested();

        // we should not build factory service provider with getting ServiceProvider in SharedFixture construction to having capability for override
        SharedFixture.WithTestConfigureServices(SetupTestConfigureServices);
        SharedFixture.WithTestConfigureAppConfiguration(
            (context, configurationBuilder) =>
            {
                SetupTestConfigureAppConfiguration(context, context.Configuration, context.HostingEnvironment);
            }
        );
        SharedFixture.WithTestConfiguration(SetupTestConfiguration);
        SharedFixture.AddOverrideEnvKeyValues(OverrideEnvKeyValues);
        SharedFixture.AddOverrideInMemoryConfig(OverrideInMemoryConfig);

        // Note: building service provider here

        // - because in IntegrationTest we don't call minimal endpoints and controllers directly our service middlewares doesn't run as a result `app.UseHeaderPropagation()` middleware won't be run which is responsible
        // for initializing `HeaderPropagationValues.Headers` and therefore in client's message handler `HeaderPropagationMessageHandler` we get an exception.
        // - won't be worked in async InitializeAsync correctly because of AsyncLocal
        InitializeHeaderPropagation();
    }

    // we use IAsyncLifetime in xunit instead of constructor when we have async operation
    public virtual async Task InitializeAsync()
    {
        // for seeding, we should run it for each test separately here. but for migration we can run it just once for all tests in shared fixture
        var seederManager = SharedFixture.ServiceProvider.GetRequiredService<IDataSeederManager>();
        // DataSeedWorker is removed from dependency injection in the test so we can't resolve it directly.
        var seedWorker = new DataSeedWorker(seederManager);

        _testWorkersRunner = new([seedWorker]);
        await _testWorkersRunner.StartWorkersAsync(CancellationToken.None);
    }

    public virtual async Task DisposeAsync()
    {
        // it is better messages delete first
        await SharedFixture.ResetDatabasesAsync(CancellationToken);

        await CancellationTokenSource.CancelAsync();

        Scope.Dispose();
    }

    protected virtual void SetupTestConfigureServices(IServiceCollection services) { }

    protected virtual void SetupTestConfigureAppConfiguration(
        WebHostBuilderContext webHostBuilderContext,
        IConfiguration configuration,
        IWebHostEnvironment hostingEnvironment
    ) { }

    protected virtual void SetupTestConfiguration(IConfiguration configurations) { }

    protected virtual void OverrideEnvKeyValues(IDictionary<string, string> keyValues) { }

    protected virtual void OverrideInMemoryConfig(IDictionary<string, string> keyValues) { }

    private void InitializeHeaderPropagation()
    {
        var headerPropagation = SharedFixture.ServiceProvider.GetRequiredService<HeaderPropagationValues>();
        headerPropagation.Headers ??= new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
    }
}

public abstract class IntegrationTestBase<TEntryPoint, TContext>(
    SharedFixtureWithEfCore<TEntryPoint, TContext> sharedFixture,
    ITestOutputHelper outputHelper
) : IntegrationTest<TEntryPoint>(sharedFixture, outputHelper)
    where TEntryPoint : class
    where TContext : DbContext
{
    public new SharedFixtureWithEfCore<TEntryPoint, TContext> SharedFixture { get; } = sharedFixture;
}

public abstract class IntegrationTestBase<TEntryPoint, TWContext, TRContext>(
    SharedFixtureWithEfCoreAndMongo<TEntryPoint, TWContext, TRContext> sharedFixture,
    ITestOutputHelper outputHelper
) : IntegrationTest<TEntryPoint>(sharedFixture, outputHelper)
    where TEntryPoint : class
    where TWContext : DbContext
    where TRContext : MongoDbContext
{
    public new SharedFixtureWithEfCoreAndMongo<TEntryPoint, TWContext, TRContext> SharedFixture { get; } =
        sharedFixture;
}
