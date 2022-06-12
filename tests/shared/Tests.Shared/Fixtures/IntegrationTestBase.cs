using System.Net.Http.Headers;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Core.Messaging.BackgroundServices;
using BuildingBlocks.Persistence.EfCore.Postgres;
using BuildingBlocks.Persistence.Mongo;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mongo2Go;
using Npgsql;
using Respawn;
using Respawn.Graph;
using Tests.Shared.Mocks;
using Tests.Shared.Mocks.Builders;

namespace Tests.Shared.Fixtures;

public abstract class IntegrationTestCore<TEntryPoint> : IAsyncLifetime
    where TEntryPoint : class
{
    private readonly Checkpoint _checkpoint;
    private readonly MongoDbRunner _mongoRunner;

    public IntegrationTestCore(IntegrationTestFixture<TEntryPoint> integrationTestFixture,
        ITestOutputHelper outputHelper)
    {
        CancellationTokenSource = new(TimeSpan.FromSeconds(Timeout));
        integrationTestFixture.Timeout = Timeout;

        IntegrationTestFixture = integrationTestFixture;
        integrationTestFixture.RegisterTestServices(RegisterTestsServices);
        integrationTestFixture.SetOutputHelper(outputHelper);

        Scope = integrationTestFixture.ServiceProvider.CreateScope();

        Logger =
            Scope.ServiceProvider.GetRequiredService<ILogger<IntegrationTestFixture<TEntryPoint>>>();

        AdminClient = integrationTestFixture.CreateClient();

        GuestClient = integrationTestFixture.CreateClient();

        UserClient = integrationTestFixture.CreateClient();

        var admin = new LoginRequestBuilder().Build();
        var user = new LoginRequestBuilder()
            .WithUserNameOrEmail(Constants.Users.User.UserName)
            .WithPassword(Constants.Users.User.Password)
            .Build();

        var adminLoginResult =
            GuestClient.PostAsJsonAsync<LoginUserRequestMock, LoginResponseMock>(Constants.LoginApi, admin)
                .GetAwaiter().GetResult();

        var userLoginResult =
            GuestClient.PostAsJsonAsync<LoginUserRequestMock, LoginResponseMock>(Constants.LoginApi, user)
                .GetAwaiter().GetResult();

        AdminClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", adminLoginResult?.AccessToken);

        UserClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", userLoginResult?.AccessToken);

        _checkpoint = new Checkpoint
        {
            // SchemasToInclude = new[] {"public"},
            DbAdapter = DbAdapter.Postgres, TablesToIgnore = new List<Table> {new("__EFMigrationsHistory"),}.ToArray()
        };
        _mongoRunner = MongoDbRunner.Start();
        var mongoOptions = integrationTestFixture.ServiceProvider.GetService<IOptions<MongoOptions>>();
        if (mongoOptions is { })
            mongoOptions.Value.ConnectionString = _mongoRunner.ConnectionString;
    }

    protected int Timeout => 180;
    protected CancellationToken CancellationToken => CancellationTokenSource.Token;
    protected CancellationTokenSource CancellationTokenSource { get; }
    protected IServiceScope Scope { get; }
    protected IntegrationTestFixture<TEntryPoint> IntegrationTestFixture { get; }
    protected ILogger Logger { get; }
    protected HttpClient AdminClient { get; }
    protected HttpClient GuestClient { get; }
    protected HttpClient UserClient { get; }

    protected virtual void RegisterTestsServices(IServiceCollection services)
    {
        var user = IntegrationTestFixture.CreateAdminUserMock();
        services.ReplaceScoped(_ => user);
    }

    public async Task InitializeAsync()
    {
        CancellationToken.ThrowIfCancellationRequested();

        await ResetState();

        await SeedData();

        await IntegrationTestFixture.ExecuteScopeAsync(async sp =>
        {
            var messagePersistenceRepository = sp.GetRequiredService<IMessagePersistenceRepository>();
            await messagePersistenceRepository.CleanupMessages();
        });

        await Scope.ServiceProvider.StartTestHostedServices(
            new[] {typeof(MessagePersistenceBackgroundService)},
            CancellationToken);
    }

    public async Task DisposeAsync()
    {
        await Scope.ServiceProvider.StopTestHostedServices(
            new[] {typeof(MessagePersistenceBackgroundService)},
            CancellationToken);

        CancellationTokenSource.Cancel();
        AdminClient.Dispose();
        GuestClient.Dispose();
        UserClient.Dispose();
        _mongoRunner.Dispose();
        Scope.Dispose();
    }

    private async Task SeedData()
    {
        using (var scope = IntegrationTestFixture.ServiceProvider.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<IDbFacadeResolver>();
            var seeders = scope.ServiceProvider.GetServices<IDataSeeder>();
            await ctx.Database.MigrateAsync(CancellationToken);

            foreach (var seeder in seeders)
            {
                await seeder.SeedAllAsync();
            }
        }
    }

    private async Task ResetState()
    {
        try
        {
            var postgresOptions = IntegrationTestFixture.ServiceProvider.GetService<IOptions<PostgresOptions>>();
            if (postgresOptions is { } && !string.IsNullOrEmpty(postgresOptions.Value.ConnectionString))
            {
                await using var conn = new NpgsqlConnection(postgresOptions.Value.ConnectionString);
                await conn.OpenAsync(CancellationToken);

                await _checkpoint.Reset(conn);
            }
        }
        catch (Exception ex)
        {
            // ignored
        }
    }
}

public abstract class IntegrationTestBase<TEntryPoint> : IntegrationTestCore<TEntryPoint>,
    IClassFixture<IntegrationTestFixture<TEntryPoint>>
    where TEntryPoint : class
{
    protected IntegrationTestBase(IntegrationTestFixture<TEntryPoint> integrationTestFixture,
        ITestOutputHelper outputHelper) : base(integrationTestFixture, outputHelper)
    {
    }
}

public abstract class IntegrationTestBase<TEntryPoint, TContext> : IntegrationTestCore<TEntryPoint>,
    IClassFixture<IntegrationTestFixture<TEntryPoint, TContext>>
    where TEntryPoint : class
    where TContext : DbContext
{
    protected IntegrationTestBase(
        IntegrationTestFixture<TEntryPoint, TContext> integrationTestFixture, ITestOutputHelper outputHelper)
        : base(integrationTestFixture, outputHelper)
    {
        IntegrationTestFixture = integrationTestFixture;
    }

    public new IntegrationTestFixture<TEntryPoint, TContext> IntegrationTestFixture { get; }
}

public abstract class
    IntegrationTestBase<TEntryPoint, TWContext, TRContext> : IntegrationTestCore<TEntryPoint>,
        IClassFixture<IntegrationTestFixture<TEntryPoint, TWContext, TRContext>>
    where TEntryPoint : class
    where TWContext : DbContext
    where TRContext : MongoDbContext
{
    protected IntegrationTestBase(
        IntegrationTestFixture<TEntryPoint, TWContext, TRContext> integrationTestFixture,
        ITestOutputHelper outputHelper) : base(integrationTestFixture, outputHelper)
    {
        IntegrationTestFixture = integrationTestFixture;
    }

    public new IntegrationTestFixture<TEntryPoint, TWContext, TRContext> IntegrationTestFixture { get; }
}
