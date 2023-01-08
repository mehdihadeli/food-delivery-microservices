using System.Net;
using System.Security.Claims;
using Ardalis.GuardClauses;
using AutoBogus;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Core.Types;
using BuildingBlocks.Persistence.Mongo;
using FluentAssertions;
using FluentAssertions.Extensions;
using MassTransit;
using MassTransit.Testing;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tests.Shared.Auth;
using Tests.Shared.Factory;
using Tests.Shared.TestBase;
using WireMock.Server;
using IBus = BuildingBlocks.Abstractions.Messaging.IBus;

namespace Tests.Shared.Fixtures;

public class SharedFixture<TEntryPoint> : IAsyncLifetime where TEntryPoint : class
{
    private ITestHarness? _harness;
    private IHttpContextAccessor? _httpContextAccessor;
    private IServiceProvider? _serviceProvider;
    private IConfiguration? _configuration;
    private HttpClient? _adminClient;
    private HttpClient? _normalClient;
    private HttpClient? _guestClient;
    private ILogger? _logger;

    public Func<Task>? OnSharedFixtureInitialized;
    public Func<Task>? OnSharedFixtureDisposed;

    public ILogger Logger =>
        _logger ??= ServiceProvider.GetRequiredService<ILogger<IntegrationTest<TEntryPoint>>>();

    public PostgresContainerFixture PostgresContainerFixture { get; }
    public Mongo2GoFixture Mongo2GoFixture { get; }
    public RabbitMQContainerFixture RabbitMqContainerFixture { get; }
    public CustomWebApplicationFactory<TEntryPoint> Factory { get; private set; }
    public IServiceProvider ServiceProvider => _serviceProvider ??= Factory.Services;

    public IConfiguration Configuration =>
        _configuration ??= ServiceProvider.GetRequiredService<IConfiguration>();

    public ITestHarness MasstransitHarness => _harness ??= ServiceProvider.GetRequiredService<ITestHarness>();

    public IHttpContextAccessor HttpContextAccessor => _httpContextAccessor ??=
        ServiceProvider.GetRequiredService<IHttpContextAccessor>();

    public HttpClient AdminHttpClient => _adminClient ??= CreateAdminHttpClient();
    public HttpClient NormalUserHttpClient => _normalClient ??= CreateNormalUserHttpClient();
    public HttpClient GuestClient => _guestClient ??= Factory.CreateClient();
    public WireMockServer WireMockServer { get; }
    public string? WireMockServerUrl { get; }

    public SharedFixture()
    {
        // Service provider will build after getting with get accessors, we don't want to build our service provider here
        PostgresContainerFixture = new PostgresContainerFixture();
        Mongo2GoFixture = new Mongo2GoFixture();
        RabbitMqContainerFixture = new RabbitMQContainerFixture();

        Factory = new CustomWebApplicationFactory<TEntryPoint>();
        AutoFaker.Configure(
            b =>
            {
                // configure global AutoBogus settings here
                b.WithRecursiveDepth(3)
                    .WithTreeDepth(1)
                    .WithRepeatCount(1);
            });

        // close to equivalency required to reconcile precision differences between EF and Postgres
        AssertionOptions.AssertEquivalencyUsing(options =>
        {
            options.Using<DateTime>(ctx => ctx.Subject
                .Should()
                .BeCloseTo(ctx.Expectation, 1.Seconds())).WhenTypeIs<DateTime>();
            options.Using<DateTimeOffset>(ctx => ctx.Subject
                .Should()
                .BeCloseTo(ctx.Expectation, 1.Seconds())).WhenTypeIs<DateTimeOffset>();

            return options;
        });

        // new WireMockServer() is equivalent to call WireMockServer.Start()
        WireMockServer = WireMockServer.Start();
        WireMockServerUrl = WireMockServer.Url;

        WithConfigureAppConfigurations((context, builder) =>
        {
            // add in-memory configuration instead of using appestings.json and override existing settings and it is accessible via IOptions and Configuration
            // https://blog.markvincze.com/overriding-configuration-in-asp-net-core-integration-tests/
            builder.AddInMemoryCollection(
                new TestConfigurations
                {
                    {"PostgresOptions:ConnectionString", PostgresContainerFixture.Container.ConnectionString},
                    {"MessagePersistenceOptions:ConnectionString", PostgresContainerFixture.Container.ConnectionString},
                    {"MongoOptions:ConnectionString", Mongo2GoFixture.MongoDbRunner.ConnectionString},
                    {"RabbitMqOptions:UserName", RabbitMqContainerFixture.Container.Username},
                    {"RabbitMqOptions:Password", RabbitMqContainerFixture.Container.Password},
                    {"RabbitMqOptions:Host", RabbitMqContainerFixture.Container.Hostname},
                    {"RabbitMqOptions:Port", RabbitMqContainerFixture.Container.Port.ToString()},
                });

            // Or we can override configuration explicitly and it is accessible via IOptions<> and Configuration
            context.Configuration["WireMockUrl"] = WireMockServerUrl;
        });
    }

    public async Task InitializeAsync()
    {
        // Service provider will build after getting with get accessors, we don't want to build our service provider here
        await Factory.InitializeAsync();
        await PostgresContainerFixture.InitializeAsync();
        await Mongo2GoFixture.InitializeAsync();
        await RabbitMqContainerFixture.InitializeAsync();

        var initCallback = OnSharedFixtureInitialized?.Invoke();
        if (initCallback != null)
            await initCallback;
    }

    public async Task DisposeAsync()
    {
        await MasstransitHarness.Stop();

        await PostgresContainerFixture.DisposeAsync();
        await Mongo2GoFixture.DisposeAsync();
        await RabbitMqContainerFixture.DisposeAsync();
        WireMockServer.Stop();
        AdminHttpClient.Dispose();
        NormalUserHttpClient.Dispose();
        GuestClient.Dispose();

        var disposeCallback = OnSharedFixtureDisposed?.Invoke();
        if (disposeCallback != null)
            await disposeCallback;

        await Factory.DisposeAsync();
    }

    public async Task CleanupMessaging(CancellationToken cancellationToken = default)
    {
        await RabbitMqContainerFixture.CleanupQueuesAsync(cancellationToken);
    }

    public async Task ResetDatabasesAsync(CancellationToken cancellationToken = default)
    {
        await PostgresContainerFixture.ResetDbAsync(cancellationToken);
        await Mongo2GoFixture.ResetDbAsync(cancellationToken);

        var mongoOptions = ServiceProvider.GetRequiredService<MongoOptions>();
        mongoOptions.ConnectionString = Mongo2GoFixture.MongoDbRunner.ConnectionString;
    }

    /// <summary>
    /// We could use `WithWebHostBuilder` method for specific config and customize existing `CustomWebApplicationFactory`
    /// </summary>
    public CustomWebApplicationFactory<TEntryPoint> WithWebHostBuilder(Action<IWebHostBuilder> builder)
    {
        Factory = Factory.WithWebHostBuilder(builder);
        return Factory;
    }

    public CustomWebApplicationFactory<TEntryPoint> WithHostBuilder(Action<IHostBuilder> builder)
    {
        Factory = Factory.WithHostBuilder(builder);
        return Factory;
    }

    public CustomWebApplicationFactory<TEntryPoint> WithConfigureAppConfigurations(
        Action<HostBuilderContext, IConfigurationBuilder> cfg)
    {
        Factory.WithConfigureAppConfigurations(cfg);
        return Factory;
    }

    public void ConfigureTestServices(Action<IServiceCollection>? services = null)
    {
        if (services is not null)
            Factory.TestConfigureServices += services;
    }

    public void SetOutputHelper(ITestOutputHelper outputHelper)
    {
        // var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
        // loggerFactory.AddXUnit(outputHelper);
        Factory.SetOutputHelper(outputHelper);
    }

    public void SetAdminUser()
    {
        var admin = CreateAdminUserMock();
        var identity = new ClaimsIdentity(admin.Claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = Substitute.For<HttpContext>();
        httpContext.User.Returns(_ => claimsPrincipal);

        var httpContextAccessor = ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = httpContext;
    }

    public async Task ExecuteScopeAsync(Func<IServiceProvider, Task> action)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        await action(scope.ServiceProvider);
    }

    public async Task<T> ExecuteScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();

        var result = await action(scope.ServiceProvider);

        return result;
    }

    public async Task<TResponse> SendAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteScopeAsync(async sp =>
        {
            var mediator = sp.GetRequiredService<IMediator>();

            return await mediator.Send(request, cancellationToken);
        });
    }

    public async Task<TResponse> SendAsync<TResponse>(
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

    public async Task SendAsync<T>(T request, CancellationToken cancellationToken = default)
        where T : class, ICommand
    {
        await ExecuteScopeAsync(async sp =>
        {
            var commandProcessor = sp.GetRequiredService<ICommandProcessor>();

            return await commandProcessor.SendAsync(request, cancellationToken);
        });
    }

    public async Task<TResponse> QueryAsync<TResponse>(
        IQuery<TResponse> query,
        CancellationToken cancellationToken = default) where TResponse : class
    {
        return await ExecuteScopeAsync(async sp =>
        {
            var queryProcessor = sp.GetRequiredService<IQueryProcessor>();

            return await queryProcessor.SendAsync(query, cancellationToken);
        });
    }

    public async ValueTask PublishMessageAsync<TMessage>(
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
    public async ValueTask WaitUntilConditionMet(Func<Task<bool>> conditionToMet, int? timeoutSecond = null)
    {
        var time = timeoutSecond ?? 300;

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

    public async Task WaitForPublishing<TMessage>(CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
    {
        await WaitUntilConditionMet(async () =>
        {
            // message has been published for this harness.
            var published = await MasstransitHarness.Published.Any<TMessage>(cancellationToken);
            // there is a fault when publishing for this harness.
            var faulty = await MasstransitHarness.Published.Any<Fault<TMessage>>(cancellationToken);

            return published & !faulty;
        });
    }

    public async Task WaitForConsuming<TMessage>(CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
    {
        await WaitUntilConditionMet(async () =>
        {
            //consumer consumed the message.
            var consumed = await MasstransitHarness.Consumed.Any<TMessage>(cancellationToken);
            //there was a fault when consuming for this harness.
            var faulty = await MasstransitHarness.Consumed.Any<Fault<TMessage>>(cancellationToken);

            return consumed && !faulty;
        });
    }

    public async Task WaitForConsuming<TMessage, TConsumedBy>(CancellationToken cancellationToken = default)
        where TMessage : class
        where TConsumedBy : class, IConsumer
    {
        var consumerHarness = ServiceProvider.GetRequiredService<IConsumerTestHarness<TConsumedBy>>();
        await WaitUntilConditionMet(async () =>
        {
            //consumer consumed the message.
            var consumed = await consumerHarness.Consumed.Any<TMessage>(cancellationToken);
            //there was a fault when consuming for this harness.
            var faulty = await consumerHarness.Consumed.Any<Fault<TMessage>>(cancellationToken);

            return consumed && !faulty;
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
    // public  async ValueTask<IHypothesis<TMessage>> ShouldConsumeWithNewConsumer<TMessage, TConsumer>(
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

    public async ValueTask ShouldProcessedOutboxPersistMessage<TMessage>(
        CancellationToken cancellationToken = default)
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
                    TypeMapper.GetFullTypeName(typeof(TMessage)) == x.DataType, cancellationToken);

                var res = filter.Any(x => x.MessageStatus == MessageStatus.Processed);

                if (res is true)
                {
                }

                return res;
            });
        });
    }

    public async ValueTask ShouldProcessedPersistInternalCommand<TInternalCommand>(
        CancellationToken cancellationToken = default)
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
                    TypeMapper.GetFullTypeName(typeof(TInternalCommand)) == x.DataType, cancellationToken);

                var res = filter.Any(x => x.MessageStatus == MessageStatus.Processed);

                return res;
            });
        });
    }

    private HttpClient CreateAdminHttpClient()
    {
        var adminClient = Factory.CreateClient();

        //https://github.com/webmotions/fake-authentication-jwtbearer/issues/14
        var claims = CreateAdminUserMock().Claims;

        adminClient.SetFakeBearerToken(claims);

        return adminClient;
    }

    private HttpClient CreateNormalUserHttpClient()
    {
        var userClient = Factory.CreateClient();

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
}
