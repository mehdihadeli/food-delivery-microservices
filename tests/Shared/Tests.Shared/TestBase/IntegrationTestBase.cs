using BuildingBlocks.Abstractions.Persistence;
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
public abstract class IntegrationTest<TEntryPoint> : XunitContextBase, IAsyncLifetime
    where TEntryPoint : class
{
    protected CancellationToken CancellationToken => CancellationTokenSource.Token;
    protected CancellationTokenSource CancellationTokenSource { get; }
    protected int Timeout => 180;
    protected IServiceScope Scope { get; }
    protected SharedFixture<TEntryPoint> SharedFixture { get; }

    protected IntegrationTest(SharedFixture<TEntryPoint> sharedFixture, ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        SharedFixture = sharedFixture;
        SharedFixture.SetOutputHelper(outputHelper);

        CancellationTokenSource = new(TimeSpan.FromSeconds(Timeout));
        CancellationToken.ThrowIfCancellationRequested();

        SharedFixture.ConfigureTestServices(RegisterTestConfigureServices);

        SharedFixture.ConfigureTestConfigureApp(
            (context, configurationBuilder) =>
            {
                RegisterTestAppConfigurations(configurationBuilder, context.Configuration, context.HostingEnvironment);
            }
        );

        // Build Service Provider here
        Scope = SharedFixture.ServiceProvider.CreateScope();
    }

    // we use IAsyncLifetime in xunit instead of constructor when we have async operation
    public virtual async Task InitializeAsync()
    {
        await RunSeedAndMigrationAsync();
    }

    public virtual async Task DisposeAsync()
    {
        // it is better messages delete first
        await SharedFixture.CleanupMessaging(CancellationToken);
        await SharedFixture.ResetDatabasesAsync(CancellationToken);

        CancellationTokenSource.Cancel();

        Scope.Dispose();
    }

    private async Task RunSeedAndMigrationAsync()
    {
        var migrations = Scope.ServiceProvider.GetServices<IMigrationExecutor>();
        var seeders = Scope.ServiceProvider.GetServices<IDataSeeder>();

        if (!SharedFixture.AlreadyMigrated)
        {
            foreach (var migration in migrations)
            {
                SharedFixture.Logger.Information("Migration '{Migration}' started...", migrations.GetType().Name);
                await migration.ExecuteAsync(CancellationToken);
                SharedFixture.Logger.Information("Migration '{Migration}' ended...", migration.GetType().Name);
            }

            SharedFixture.AlreadyMigrated = true;
        }

        foreach (var seeder in seeders)
        {
            SharedFixture.Logger.Information("Seeder '{Seeder}' started...", seeder.GetType().Name);
            await seeder.SeedAllAsync();
            SharedFixture.Logger.Information("Seeder '{Seeder}' ended...", seeder.GetType().Name);
        }
    }

    protected virtual void RegisterTestConfigureServices(IServiceCollection services) { }

    protected virtual void RegisterTestAppConfigurations(
        IConfigurationBuilder builder,
        IConfiguration configuration,
        IHostEnvironment environment
    ) { }
}

public abstract class IntegrationTestBase<TEntryPoint, TContext> : IntegrationTest<TEntryPoint>
    where TEntryPoint : class
    where TContext : DbContext
{
    protected IntegrationTestBase(
        SharedFixtureWithEfCore<TEntryPoint, TContext> sharedFixture,
        ITestOutputHelper outputHelper
    )
        : base(sharedFixture, outputHelper)
    {
        SharedFixture = sharedFixture;
    }

    public new SharedFixtureWithEfCore<TEntryPoint, TContext> SharedFixture { get; }
}

public abstract class IntegrationTestBase<TEntryPoint, TWContext, TRContext> : IntegrationTest<TEntryPoint>
    where TEntryPoint : class
    where TWContext : DbContext
    where TRContext : MongoDbContext
{
    protected IntegrationTestBase(
        SharedFixtureWithEfCoreAndMongo<TEntryPoint, TWContext, TRContext> sharedFixture,
        ITestOutputHelper outputHelper
    )
        : base(sharedFixture, outputHelper)
    {
        SharedFixture = sharedFixture;
    }

    public new SharedFixtureWithEfCoreAndMongo<TEntryPoint, TWContext, TRContext> SharedFixture { get; }
}
