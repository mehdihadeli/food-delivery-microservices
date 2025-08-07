using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Persistence.EfCore.Postgres;
using BuildingBlocks.Persistence.Mongo;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tests.Shared.Fixtures;
using Xunit;

namespace Tests.Shared.TestBase;

//https://bartwullems.blogspot.com/2019/09/xunit-async-lifetime.html
//https://www.danclarke.com/cleaner-tests-with-iasynclifetime
//https://xunit.net/docs/shared-context
public abstract class IntegrationTest<TEntryPoint> : IAsyncLifetime
    where TEntryPoint : class
{
    private IServiceScope? _serviceScope;

    protected CancellationToken CancellationToken => TestContext.Current.CancellationToken;
    protected int Timeout => 180;

    // Build Service Provider here
    protected IServiceScope Scope => _serviceScope ??= SharedFixture.ServiceProvider.CreateScope();
    protected SharedFixture<TEntryPoint> SharedFixture { get; }

    protected IntegrationTest(SharedFixture<TEntryPoint> sharedFixture)
    {
        SharedFixture = sharedFixture;
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

        // Note: building service provider here or InitializeAsync
    }

    // we use IAsyncLifetime in xunit instead of constructor when we have an async operation
    public virtual async ValueTask InitializeAsync()
    {
        // Note: building service provider here
        var testSeeders = SharedFixture.ServiceProvider.GetServices<ITestDataSeeder>();
        foreach (var testDataSeeder in testSeeders)
        {
            await testDataSeeder.SeedAsync();
        }
    }

    public virtual async ValueTask DisposeAsync()
    {
        // cleanup data and messages in each test
        await SharedFixture.CleanupAsync(CancellationToken);

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
}

public abstract class IntegrationTestBase<TEntryPoint, TContext>(
    SharedFixtureWithEfCore<TEntryPoint, TContext> sharedFixture
) : IntegrationTest<TEntryPoint>(sharedFixture)
    where TEntryPoint : class
    where TContext : DbContext
{
    public new SharedFixtureWithEfCore<TEntryPoint, TContext> SharedFixture { get; } = sharedFixture;
}

public abstract class IntegrationTestBase<TEntryPoint, TWContext, TRContext>(
    SharedFixtureWithEfCoreAndMongo<TEntryPoint, TWContext, TRContext> sharedFixture
) : IntegrationTestBase<TEntryPoint, TWContext>(sharedFixture)
    where TEntryPoint : class
    where TWContext : DbContext
    where TRContext : MongoDbContext
{
    public new SharedFixtureWithEfCoreAndMongo<TEntryPoint, TWContext, TRContext> SharedFixture { get; } =
        sharedFixture;
}
