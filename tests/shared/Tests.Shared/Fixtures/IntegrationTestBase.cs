using System.Net;
using System.Security.Claims;
using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Abstractions.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Messaging.BackgroundServices;
using BuildingBlocks.Core.Messaging.MessagePersistence;
using BuildingBlocks.Core.Types;
using BuildingBlocks.Messaging.Persistence.Postgres.MessagePersistence;
using BuildingBlocks.Persistence.Mongo;
using Core.Persistence.Postgres;
using MassTransit;
using MassTransit.Testing;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mongo2Go;
using Respawn;
using Respawn.Graph;
using Tests.Shared.Auth;
using Tests.Shared.Probing;
using WireMock.Server;
using IBus = BuildingBlocks.Abstractions.Messaging.IBus;

namespace Tests.Shared.Fixtures;

//https://bartwullems.blogspot.com/2019/09/xunit-async-lifetime.html
//https://www.danclarke.com/cleaner-tests-with-iasynclifetime
//https://xunit.net/docs/shared-context
[Trait("Category", "Integration")]
public abstract class IntegrationTestCore<TEntryPoint> : XunitContextBase, IAsyncLifetime,
    IClassFixture<CustomWebApplicationFactory<TEntryPoint>>, IClassFixture<SharedFixture>
    where TEntryPoint : class
{
    private MongoDbRunner? _mongoRunner;
    private CustomWebApplicationFactory<TEntryPoint> _factory;
    public int Timeout => 180;
    public IServiceProvider ServiceProvider { get; }
    public IConfiguration Configuration { get; }

    // TestHarness will register as singleton
    public ITestHarness Harness { get; }
    public WireMockServer WireMockServer { get; }
    public string? WireMockServerUrl { get; }
    public IHttpContextAccessor HttpContextAccessor { get; }
    protected CancellationToken CancellationToken => CancellationTokenSource.Token;
    protected CancellationTokenSource CancellationTokenSource { get; }
    protected IServiceScope Scope { get; }
    protected ILogger Logger { get; }
    protected HttpClient AdminHttpClient { get; private set; } = default!;
    protected HttpClient NormalUserHttpClient { get; private set; } = default!;
    protected HttpClient HttpClient { get; private set; } = default!;

    protected IntegrationTestCore(
        CustomWebApplicationFactory<TEntryPoint> factory,
        SharedFixture sharedFixture,
        ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _factory = factory;
        SetOutputHelper(outputHelper);
        var containersFixture1 = sharedFixture.ContainersFixture;
        WireMockServer = sharedFixture.WireMockServer;
        WireMockServerUrl = WireMockServer.Url;
        CancellationTokenSource = new(TimeSpan.FromSeconds(Timeout));

        ConfigureTestServices(services =>
        {
            services.RemoveAll<IEfConnectionFactory>();
            services.AddScoped<IEfConnectionFactory>(_ =>
                new EfNpgsqlConnectionFactory(containersFixture1.PostgresTestContainer.ConnectionString));

            services.RemoveAll<IMessagePersistenceConnectionFactory>();
            services.AddScoped<IMessagePersistenceConnectionFactory>(_ =>
                new MessagePersistenceConnectionFactory(containersFixture1.PostgresTestContainer.ConnectionString));
        });

        ConfigureTestAppConfigurations((context, builder) =>
        {
            // add in-memory configuration instead of using appestings.json and override existing settings and it is accessible via IOptions and Configuration
            // https://blog.markvincze.com/overriding-configuration-in-asp-net-core-integration-tests/
            builder.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    {"PostgresOptions:ConnectionString", containersFixture1.PostgresTestContainer.ConnectionString},
                    {
                        "MessagePersistenceOptions:ConnectionString",
                        containersFixture1.PostgresTestContainer.ConnectionString
                    },
                });

            // Or we can override configuration explicitly and it is accessible via IOptions<> and Configuration
            context.Configuration["WireMockUrl"] = WireMockServer.Url;
        });

        ConfigureTestApp();

        // Service provider will build here
        ServiceProvider = _factory.Services;
        Configuration = _factory.Configuration;

        Harness = ServiceProvider.GetRequiredService<ITestHarness>();

        HttpContextAccessor = ServiceProvider.GetRequiredService<IHttpContextAccessor>();

        CancellationToken.ThrowIfCancellationRequested();

        Scope = ServiceProvider.CreateScope();

        Logger =
            Scope.ServiceProvider.GetRequiredService<ILogger<IntegrationTestCore<TEntryPoint>>>();
    }

    // we use IAsyncLifetime in xunit instead of constructor when we have async operation
    public virtual async Task InitializeAsync()
    {
        await ConfigureHttpClients();

        await ResetPostgresState();

        InitializeMongoDb();

        await ExecuteScopeAsync(async sp =>
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
        await Scope.ServiceProvider.StopTestHostedServices(
            new[] {typeof(MessagePersistenceBackgroundService)},
            CancellationToken);

        CancellationTokenSource.Cancel();
        AdminHttpClient.Dispose();
        NormalUserHttpClient.Dispose();
        WireMockServer.Stop();
        HttpClient.Dispose();
        _mongoRunner?.Dispose();
        Scope.Dispose();
    }

    /// <summary>
    /// We could use `WithWebHostBuilder` method for specific config and customize existing `CustomWebApplicationFactory`
    /// </summary>
    protected CustomWebApplicationFactory<TEntryPoint> WithWebHostBuilder(Action<IWebHostBuilder> builder)
    {
        _factory = _factory.WithWebHostBuilder(builder);
        return _factory;
    }

    protected CustomWebApplicationFactory<TEntryPoint> WithAppConfigurations(
        Action<WebHostBuilderContext, IConfigurationBuilder> cfg)
    {
        _factory.WithConfigureAppConfigurations(cfg);
        return _factory;
    }

    private void ConfigureTestServices(Action<IServiceCollection>? services = null)
    {
        if (services is not null)
            _factory.TestConfigureServices += services;
        _factory.TestConfigureServices += RegisterTestConfigureServices;
    }

    private void ConfigureTestApp(Action<IApplicationBuilder>? appBuilder = null)
    {
        if (appBuilder is not null)
            _factory.TestConfigureApp += appBuilder;
        _factory.TestConfigureApp += RegisterTestConfigureApp;
    }

    private void ConfigureTestAppConfigurations(Action<WebHostBuilderContext, IConfigurationBuilder>? builder = null)
    {
        if (builder is not null)
            _factory.WithConfigureAppConfigurations(builder);

        _factory.WithConfigureAppConfigurations((context, configurationBuilder) =>
        {
            RegisterTestAppConfigurations(configurationBuilder, context.Configuration, context.HostingEnvironment);
        });
    }

    protected virtual void RegisterTestConfigureServices(IServiceCollection services)
    {
    }

    protected virtual void RegisterTestConfigureApp(IApplicationBuilder app)
    {
    }

    protected virtual void RegisterTestAppConfigurations(
        IConfigurationBuilder builder,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
    }

    public async Task AssertEventually(IProbe probe, int timeout)
    {
        await new Poller(timeout).CheckAsync(probe);
    }

    protected async ValueTask ExecuteScopeAsync(Func<IServiceProvider, ValueTask> action)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        await action(scope.ServiceProvider);
    }

    protected async ValueTask<T> ExecuteScopeAsync<T>(Func<IServiceProvider, ValueTask<T>> action)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();

        var result = await action(scope.ServiceProvider);

        return result;
    }

    protected void SetAdminUser()
    {
        var admin = CreateAdminUserMock();
        var identity = new ClaimsIdentity(admin.Claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = Substitute.For<HttpContext>();
        httpContext.User.Returns(_ => claimsPrincipal);

        var httpContextAccessor = ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = httpContext;
    }

    protected async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        return await ExecuteScopeAsync(async sp =>
        {
            var mediator = sp.GetRequiredService<IMediator>();

            return await mediator.Send(request, CancellationToken);
        });
    }

    protected async Task<TResponse> SendAsync<TResponse>(
        ICommand<TResponse> request,
        CancellationToken cancellationToken = default)
        where TResponse : notnull
    {
        return await ExecuteScopeAsync(async sp =>
        {
            var commandProcessor = sp.GetRequiredService<ICommandProcessor>();

            return await commandProcessor.SendAsync(request, cancellationToken);
        });
    }

    protected async Task SendAsync<T>(T request, CancellationToken cancellationToken = default)
        where T : class, ICommand
    {
        await ExecuteScopeAsync(async sp =>
        {
            var commandProcessor = sp.GetRequiredService<ICommandProcessor>();

            return await commandProcessor.SendAsync(request, cancellationToken);
        });
    }

    protected async Task<TResponse> QueryAsync<TResponse>(
        IQuery<TResponse> query,
        CancellationToken cancellationToken = default) where TResponse : class
    {
        return await ExecuteScopeAsync(async sp =>
        {
            var queryProcessor = sp.GetRequiredService<IQueryProcessor>();

            return await queryProcessor.SendAsync(query, cancellationToken);
        });
    }

    protected async ValueTask PublishMessageAsync<TMessage>(
        TMessage message,
        IDictionary<string, object?>? headers = null,
        CancellationToken cancellationToken = default)
        where
        TMessage : class, IMessage
    {
        await ExecuteScopeAsync(async sp =>
        {
            var bus = sp.GetRequiredService<IBus>();

            await bus.PublishAsync(message, headers, cancellationToken);
        });
    }

    // Ref: https://tech.energyhelpline.com/in-memory-testing-with-masstransit/
    protected async ValueTask WaitUntilConditionMet(Func<Task<bool>> conditionToMet, int? timeoutSecond = null)
    {
        var time = timeoutSecond ?? Timeout;

        var startTime = DateTime.Now;
        var timeoutExpired = false;
        var meet = await conditionToMet.Invoke();
        while (!meet)
        {
            if (timeoutExpired)
            {
                throw new TimeoutException("Condition not met for the test.");
            }

            await Task.Delay(100);
            meet = await conditionToMet.Invoke();
            timeoutExpired = DateTime.Now - startTime > TimeSpan.FromSeconds(time);
        }
    }

    protected async Task WaitForPublishing<TMessage>()
        where TMessage : class, IMessage
    {
        await WaitUntilConditionMet(async () =>
        {
            // message has been published for this harness.
            var published = await Harness.Published.Any<TMessage>(CancellationToken);
            // there is a fault when publishing for this harness.
            var faulty = await Harness.Published.Any<Fault<TMessage>>(CancellationToken);

            return published && faulty == false;
        });
    }

    protected async Task WaitForConsuming<TMessage>()
        where TMessage : class, IMessage
    {
        await WaitUntilConditionMet(async () =>
        {
            //consumer consumed the message.
            var consumed = await Harness.Consumed.Any<TMessage>(CancellationToken);
            //there was a fault when consuming for this harness.
            var faulty = await Harness.Consumed.Any<Fault<TMessage>>(CancellationToken);

            return consumed && faulty == false;
        });
    }

    protected async Task WaitForConsuming<TMessage, TConsumedBy>()
        where TMessage : class
        where TConsumedBy : class, IConsumer
    {
        var consumerHarness = ServiceProvider.GetRequiredService<IConsumerTestHarness<TConsumedBy>>();
        await WaitUntilConditionMet(async () =>
        {
            //consumer consumed the message.
            var consumed = await consumerHarness.Consumed.Any<TMessage>(CancellationToken);
            //there was a fault when consuming for this harness.
            var faulty = await consumerHarness.Consumed.Any<Fault<TMessage>>(CancellationToken);

            return consumed && faulty == false;
        });
    }

    // public async ValueTask<IHypothesis<TMessage>> ShouldConsumeWithNewConsumer<TMessage>(
    //     Predicate<TMessage>? match = null)
    //     where TMessage : class, IMessage
    // {
    //     var hypothesis = Hypothesis
    //         .For<TMessage>()
    //         .Any(match ?? (_ => true));
    //
    //     ////https://stackoverflow.com/questions/55169197/how-to-use-masstransit-test-harness-to-test-consumer-with-constructor-dependency
    //     // Harness.Consumer(() => hypothesis.AsConsumer());
    //
    //     await Harness.SubscribeHandler<TMessage>(ctx =>
    //     {
    //         hypothesis.Test(ctx.Message).GetAwaiter().GetResult();
    //         return true;
    //     });
    //
    //     return hypothesis;
    // }
    //
    // public async ValueTask<IHypothesis<TMessage>> ShouldConsumeWithNewConsumer<TMessage, TConsumer>(
    //     Predicate<TMessage>? match = null)
    //     where TMessage : class, IMessage
    //     where TConsumer : class, IConsumer<TMessage>
    // {
    //     var hypothesis = Hypothesis
    //         .For<TMessage>()
    //         .Any(match ?? (_ => true));
    //
    //     //https://stackoverflow.com/questions/55169197/how-to-use-masstransit-test-harness-to-test-consumer-with-constructor-dependency
    //     Harness.Consumer(() => hypothesis.AsConsumer<TMessage, TConsumer>(ServiceProvider));
    //
    //     return hypothesis;
    // }

    protected async ValueTask ShouldProcessedOutboxPersistMessage<TMessage>()
        where TMessage : class, IMessage
    {
        await WaitUntilConditionMet(async () =>
        {
            return await ExecuteScopeAsync(async sp =>
            {
                var messagePersistenceService = sp.GetService<IMessagePersistenceService>();
                Guard.Against.Null(messagePersistenceService, nameof(messagePersistenceService));

                var filter = await messagePersistenceService.GetByFilterAsync(x =>
                    x.DeliveryType == MessageDeliveryType.Outbox &&
                    TypeMapper.GetFullTypeName(typeof(TMessage)) == x.DataType, CancellationToken);

                var res = filter.Any(x => x.MessageStatus == MessageStatus.Processed);

                if (res is true)
                {
                }

                return res;
            });
        });
    }

    protected async ValueTask ShouldProcessedPersistInternalCommand<TInternalCommand>()
        where TInternalCommand : class, IInternalCommand
    {
        await WaitUntilConditionMet(async () =>
        {
            return await ExecuteScopeAsync(async sp =>
            {
                var messagePersistenceService = sp.GetService<IMessagePersistenceService>();
                Guard.Against.Null(messagePersistenceService, nameof(messagePersistenceService));

                var filter = await messagePersistenceService.GetByFilterAsync(x =>
                    x.DeliveryType == MessageDeliveryType.Internal &&
                    TypeMapper.GetFullTypeName(typeof(TInternalCommand)) == x.DataType, CancellationToken);

                var res = filter.Any(x => x.MessageStatus == MessageStatus.Processed);

                return res;
            });
        });
    }

    private void InitializeMongoDb()
    {
        _mongoRunner = MongoDbRunner.Start();
        var mongoOptions = ServiceProvider.GetService<IOptions<MongoOptions>>();
        if (mongoOptions is { })
            mongoOptions.Value.ConnectionString = _mongoRunner.ConnectionString;
    }

    private async Task ConfigureHttpClients()
    {
        HttpClient = _factory.CreateClient();
        AdminHttpClient = CreateAdminHttpClient();
        NormalUserHttpClient = CreateNormalUserHttpClient();
    }

    private async Task ResetPostgresState()
    {
        try
        {
            //_connectionString = ConfigurationHelper.GetOptions<PostgresOptions>().ConnectionString;
            using var connectionFactory = Scope.ServiceProvider.GetRequiredService<IEfConnectionFactory>();

            var connection = await connectionFactory.GetOrCreateConnectionAsync();
            var reSpawner = await Respawner.CreateAsync(connection,
                new RespawnerOptions
                {
                    TablesToIgnore = new Table[] {"__EFMigrationsHistory"},
                    SchemasToExclude = new[] {"information_schema", "pg_subscription", "pg_catalog", "pg_toast"},
                    DbAdapter = DbAdapter.Postgres
                });
            await reSpawner.ResetAsync(connection)!;
        }
        catch (Exception e)
        {
            throw new Exception("Error in ResetPostgresState of ReSpawner.");
        }
    }

    private HttpClient CreateAdminHttpClient()
    {
        var adminClient = _factory.CreateClient();

        //https://github.com/webmotions/fake-authentication-jwtbearer/issues/14
        var claims = CreateAdminUserMock().Claims;

        adminClient.SetFakeBearerToken(claims);

        return adminClient;
    }

    private HttpClient CreateNormalUserHttpClient()
    {
        var userClient = _factory.CreateClient();

        //https://github.com/webmotions/fake-authentication-jwtbearer/issues/14
        var claims = CreateNormalUserMock().Claims;

        userClient.SetFakeBearerToken(claims);

        return userClient;
    }

    private MockAuthUser CreateAdminUserMock()
    {
        var roleClaim = new Claim(ClaimTypes.Role, Constants.Users.Admin.Role);
        var otherClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Constants.Users.Admin.UserId),
            new(ClaimTypes.Name, Constants.Users.Admin.UserName),
            new(ClaimTypes.Email, Constants.Users.Admin.Email)
        };

        return _ = new MockAuthUser(otherClaims.Concat(new[] {roleClaim}).ToArray());
    }

    private MockAuthUser CreateNormalUserMock()
    {
        var roleClaim = new Claim(ClaimTypes.Role, Constants.Users.NormalUser.Role);
        var otherClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Constants.Users.NormalUser.UserId),
            new(ClaimTypes.Name, Constants.Users.NormalUser.UserName),
            new(ClaimTypes.Email, Constants.Users.NormalUser.Email)
        };

        return _ = new MockAuthUser(otherClaims.Concat(new[] {roleClaim}).ToArray());
    }

    private void SetOutputHelper(ITestOutputHelper outputHelper)
    {
        // var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
        // loggerFactory.AddXUnit(outputHelper);
        _factory.SetOutputHelper(outputHelper);
    }
}

public abstract class IntegrationTestBase<TEntryPoint> : IntegrationTestCore<TEntryPoint>
    where TEntryPoint : class
{
    protected IntegrationTestBase(
        CustomWebApplicationFactory<TEntryPoint> webApplicationFactory,
        SharedFixture sharedFixture,
        ITestOutputHelper outputHelper)
        : base(webApplicationFactory, sharedFixture, outputHelper)
    {
    }
}

public abstract class IntegrationTestBase<TEntryPoint, TContext> : IntegrationTestCore<TEntryPoint>
    where TEntryPoint : class
    where TContext : DbContext
{
    protected IntegrationTestBase(
        CustomWebApplicationFactory<TEntryPoint> webApplicationFactory,
        SharedFixture sharedFixture,
        ITestOutputHelper outputHelper)
        : base(webApplicationFactory, sharedFixture, outputHelper)
    {
    }

    protected async Task ExecuteTxContextAsync(Func<IServiceProvider, TContext, ValueTask> action)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            try
            {
                await dbContext.Database.BeginTransactionAsync();

                await action(scope.ServiceProvider, dbContext);

                await dbContext.Database.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                dbContext.Database?.RollbackTransactionAsync();
                throw;
            }
        });
    }

    protected async Task<T> ExecuteTxContextAsync<T>(Func<IServiceProvider, TContext, ValueTask<T>> action)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        //https://weblogs.asp.net/dixin/entity-framework-core-and-linq-to-entities-7-data-changes-and-transactions
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            try
            {
                await dbContext.Database.BeginTransactionAsync();

                var result = await action(scope.ServiceProvider, dbContext);

                await dbContext.Database.CommitTransactionAsync();

                return result;
            }
            catch (Exception ex)
            {
                dbContext.Database?.RollbackTransactionAsync();
                throw;
            }
        });
    }

    protected ValueTask ExecuteContextAsync(Func<TContext, ValueTask> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TContext>()));

    protected ValueTask ExecuteContextAsync(Func<TContext, ICommandProcessor, ValueTask> action)
        => ExecuteScopeAsync(sp =>
            action(sp.GetRequiredService<TContext>(), sp.GetRequiredService<ICommandProcessor>()));

    protected ValueTask<T> ExecuteContextAsync<T>(Func<TContext, ValueTask<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TContext>()));

    protected ValueTask<T> ExecuteContextAsync<T>(Func<TContext, ICommandProcessor, ValueTask<T>> action)
        => ExecuteScopeAsync(sp =>
            action(sp.GetRequiredService<TContext>(), sp.GetRequiredService<ICommandProcessor>()));

    protected async ValueTask<int> InsertAsync<T>(params T[] entities) where T : class
    {
        return await ExecuteContextAsync(async db =>
        {
            foreach (var entity in entities)
            {
                db.Set<T>().Add(entity);
            }

            return await db.SaveChangesAsync();
        });
    }

    protected async ValueTask<int> InsertAsync<TEntity>(TEntity entity) where TEntity : class
    {
        return await ExecuteContextAsync(async db =>
        {
            db.Set<TEntity>().Add(entity);

            return await db.SaveChangesAsync();
        });
    }

    protected async ValueTask<int> InsertAsync<TEntity, TEntity2>(TEntity entity, TEntity2 entity2)
        where TEntity : class
        where TEntity2 : class
    {
        return await ExecuteContextAsync(async db =>
        {
            db.Set<TEntity>().Add(entity);
            db.Set<TEntity2>().Add(entity2);

            return await db.SaveChangesAsync();
        });
    }

    protected async ValueTask<int> InsertAsync<TEntity, TEntity2, TEntity3>(TEntity entity, TEntity2 entity2, TEntity3
        entity3)
        where TEntity : class
        where TEntity2 : class
        where TEntity3 : class
    {
        return await ExecuteContextAsync(async db =>
        {
            db.Set<TEntity>().Add(entity);
            db.Set<TEntity2>().Add(entity2);
            db.Set<TEntity3>().Add(entity3);

            return await db.SaveChangesAsync();
        });
    }

    protected async ValueTask<int> InsertAsync<TEntity, TEntity2, TEntity3, TEntity4>(TEntity entity, TEntity2 entity2,
        TEntity3 entity3, TEntity4 entity4)
        where TEntity : class
        where TEntity2 : class
        where TEntity3 : class
        where TEntity4 : class
    {
        return await ExecuteContextAsync(async db =>
        {
            db.Set<TEntity>().Add(entity);
            db.Set<TEntity2>().Add(entity2);
            db.Set<TEntity3>().Add(entity3);
            db.Set<TEntity4>().Add(entity4);

            return await db.SaveChangesAsync();
        });
    }

    protected ValueTask<T?> FindAsync<T>(object id) where T : class
    {
        return ExecuteContextAsync(db => db.Set<T>().FindAsync(id));
    }
}

public abstract class
    IntegrationTestBase<TEntryPoint, TWContext, TRContext> : IntegrationTestBase<TEntryPoint, TWContext>
    where TEntryPoint : class
    where TWContext : DbContext
    where TRContext : MongoDbContext
{
    protected IntegrationTestBase(
        CustomWebApplicationFactory<TEntryPoint> webApplicationFactory,
        SharedFixture sharedFixture,
        ITestOutputHelper outputHelper)
        : base(webApplicationFactory, sharedFixture, outputHelper)
    {
    }

    protected ValueTask ExecuteReadContextAsync(Func<TRContext, ValueTask> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TRContext>()));

    protected ValueTask ExecuteReadContextAsync(Func<TRContext, ICommandProcessor, ValueTask> action)
        => ExecuteScopeAsync(sp =>
            action(sp.GetRequiredService<TRContext>(), sp.GetRequiredService<ICommandProcessor>()));

    protected ValueTask<T> ExecuteReadContextAsync<T>(Func<TRContext, ValueTask<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TRContext>()));

    protected ValueTask<T> ExecuteReadContextAsync<T>(Func<TRContext, ICommandProcessor, ValueTask<T>> action)
        => ExecuteScopeAsync(sp =>
            action(sp.GetRequiredService<TRContext>(), sp.GetRequiredService<ICommandProcessor>()));
}
