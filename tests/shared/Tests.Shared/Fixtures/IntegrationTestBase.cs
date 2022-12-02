using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Core.Messaging.BackgroundServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BuildingBlocks.Persistence.Mongo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tests.Shared.Extensions;

namespace Tests.Shared.Fixtures;

//https://bartwullems.blogspot.com/2019/09/xunit-async-lifetime.html
//https://www.danclarke.com/cleaner-tests-with-iasynclifetime
//https://xunit.net/docs/shared-context
[Trait("Category", "Integration")]
public abstract class IntegrationTest<TEntryPoint> : IAsyncLifetime
    where TEntryPoint : class
{
    protected CancellationToken CancellationToken => CancellationTokenSource.Token;
    protected CancellationTokenSource CancellationTokenSource { get; }
    protected IServiceScope Scope { get; }
    protected ILogger Logger { get; }

    protected SharedFixture<TEntryPoint> Fixture { get; }

    protected IntegrationTest(
        SharedFixture<TEntryPoint> sharedFixture,
        ITestOutputHelper outputHelper)
    {
        Fixture = sharedFixture;
        Fixture.SetOutputHelper(outputHelper);
        Fixture.Timeout = 200;
        CancellationTokenSource = new(TimeSpan.FromSeconds(Fixture.Timeout));

        CancellationToken.ThrowIfCancellationRequested();
        Fixture.SetOutputHelper(outputHelper);

        Fixture.WithConfigureAppConfigurations((context, configurationBuilder) =>
        {
            RegisterTestAppConfigurations(configurationBuilder, context.Configuration, context.HostingEnvironment);
        });

        Fixture.ConfigureTestServices(RegisterTestConfigureServices);

        // Build Service Provider here
        Scope = Fixture.ServiceProvider.CreateScope();

        Logger =
            Scope.ServiceProvider.GetRequiredService<ILogger<IntegrationTest<TEntryPoint>>>();
    }

    // we use IAsyncLifetime in xunit instead of constructor when we have async operation
    public virtual async Task InitializeAsync()
    {
        await Fixture.ResetDatabasesAsync(CancellationToken);

        await Fixture.ExecuteScopeAsync(async sp =>
        {
            var messagePersistenceRepository = sp.GetRequiredService<IMessagePersistenceRepository>();
            await messagePersistenceRepository.CleanupMessages();
        });

        await Scope.ServiceProvider.StartTestHostedServices(
            new[] {typeof(MessagePersistenceBackgroundService)},
            CancellationToken);
    }

    public virtual async Task DisposeAsync()
    {
        await Fixture.CleanupMessaging(CancellationToken);

        await Scope.ServiceProvider.StopTestHostedServices(
            new[] {typeof(MessagePersistenceBackgroundService)},
            CancellationToken);

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
        SharedFixture<TEntryPoint, TContext> sharedFixture, ITestOutputHelper outputHelper)
        : base(sharedFixture, outputHelper)
    {
        Fixture = sharedFixture;
    }

    public new SharedFixture<TEntryPoint, TContext> Fixture { get; }
}

public abstract class
    IntegrationTestBase<TEntryPoint, TWContext, TRContext> : IntegrationTest<TEntryPoint>
    where TEntryPoint : class
    where TWContext : DbContext
    where TRContext : MongoDbContext
{
    protected IntegrationTestBase(
        SharedFixture<TEntryPoint, TWContext, TRContext> sharedFixture,
        ITestOutputHelper outputHelper) : base(sharedFixture, outputHelper)
    {
        Fixture = sharedFixture;
    }

    public new SharedFixture<TEntryPoint, TWContext, TRContext> Fixture { get; }
}
