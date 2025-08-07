using System.Net.Http.Headers;
using System.Security.Claims;
using AutoBogus;
using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Abstractions.Messages.MessagePersistence;
using BuildingBlocks.Abstractions.Queries;
using BuildingBlocks.Caching;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Messages.MessagePersistence;
using BuildingBlocks.Core.Messages.MessagePersistence.BackgroundServices;
using BuildingBlocks.Core.Persistence;
using BuildingBlocks.Core.Types;
using BuildingBlocks.Integration.MassTransit;
using BuildingBlocks.Persistence.EfCore.Postgres;
using BuildingBlocks.Persistence.Mongo;
using FluentAssertions;
using FluentAssertions.Extensions;
using MassTransit;
using MassTransit.Internals.Caching;
using MassTransit.Testing;
using Mediator;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using NSubstitute;
using Serilog;
using Tests.Shared.Auth;
using Tests.Shared.Extensions;
using Tests.Shared.Factory;
using WireMock.Server;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;
using ICommand = BuildingBlocks.Abstractions.Commands.ICommand;
using IExternalEventBus = BuildingBlocks.Abstractions.Messages.IExternalEventBus;
using IMessage = BuildingBlocks.Abstractions.Messages.IMessage;

namespace Tests.Shared.Fixtures;

public class SharedFixture<TEntryPoint> : IAsyncLifetime
    where TEntryPoint : class
{
    private readonly IMessageSink _messageSink;
    private ITestHarness? _harness;
    private IHttpContextAccessor? _httpContextAccessor;
    private IServiceProvider? _serviceProvider;
    private IConfiguration? _configuration;
    private HttpClient? _adminClient;
    private HttpClient? _normalClient;
    private HttpClient? _guestClient;

    public WireMockServer WireMockServer { get; }
    public string WireMockServerUrl { get; }
    public event Func<Task>? SharedFixtureInitialized;
    public event Func<Task>? SharedFixtureDisposed;
    public ILogger Logger { get; }
    public PostgresContainerFixture PostgresContainerFixture { get; }
    public RedisContainerFixture RedisContainerFixture { get; }
    public MongoContainerFixture MongoContainerFixture { get; }
    public RabbitMQContainerFixture RabbitMqContainerFixture { get; }
    public CustomWebApplicationFactory<TEntryPoint> Factory { get; private set; }
    public IServiceProvider ServiceProvider => _serviceProvider ??= Factory.Services;

    public IConfiguration Configuration => _configuration ??= ServiceProvider.GetRequiredService<IConfiguration>();

    public ITestHarness MasstransitHarness => _harness ??= ServiceProvider.GetRequiredService<ITestHarness>();

    public IHttpContextAccessor HttpContextAccessor =>
        _httpContextAccessor ??= ServiceProvider.GetRequiredService<IHttpContextAccessor>();

    /// <summary>
    /// We should not dispose this GuestClient, because we reuse it in our tests
    /// </summary>
    public HttpClient GuestClient
    {
        get
        {
            if (_guestClient == null)
            {
                _guestClient = Factory.CreateClient();
                // Set the media type of the request to JSON - we need this for getting problem details result for all http calls because problem details just return response for request with media type JSON
                _guestClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            return _guestClient;
        }
    }

    /// <summary>
    /// We should not dispose this AdminHttpClient, because we reuse it in our tests
    /// </summary>
    public HttpClient AdminHttpClient => _adminClient ??= CreateAdminHttpClient();

    /// <summary>
    /// We should not dispose this NormalUserHttpClient, because we reuse it in our tests
    /// </summary>
    public HttpClient NormalUserHttpClient => _normalClient ??= CreateNormalUserHttpClient();

    //https://github.com/xunit/xunit/issues/565
    //https://github.com/xunit/xunit/pull/1705
    //https://xunit.net/docs/capturing-output#output-in-extensions
    //https://andrewlock.net/tracking-down-a-hanging-xunit-test-in-ci-building-a-custom-test-framework/
    public SharedFixture(IMessageSink messageSink)
    {
        _messageSink = messageSink;
        messageSink.OnMessage(new DiagnosticMessage("Constructing SharedFixture..."));

        // //https://github.com/trbenning/serilog-sinks-xunit
        // Logger = new LoggerConfiguration()
        //     .MinimumLevel.Verbose()
        //     .WriteTo.TestOutput(messageSink)
        //     .CreateLogger()
        //     .ForContext<SharedFixture<TEntryPoint>>();

        // //https://github.com/testcontainers/testcontainers-dotnet/blob/8db93b2eb28bc2bc7d579981da1651cd41ec03f8/docs/custom_configuration/index.md#enable-logging
        // //// TODO: Breaking change in the testcontainer upgrade
        // TestcontainersSettings.Logger = new Serilog.Extensions.Logging.SerilogLoggerFactory(Logger).CreateLogger(
        //     "TestContainer"
        // );

        // Service provider will build after getting with get accessors, we don't want to build our service provider here
        PostgresContainerFixture = new PostgresContainerFixture(messageSink);
        RedisContainerFixture = new RedisContainerFixture(messageSink);
        MongoContainerFixture = new MongoContainerFixture(messageSink);
        RabbitMqContainerFixture = new RabbitMQContainerFixture(messageSink);

        AutoFaker.Configure(b =>
        {
            // configure global AutoBogus settings here
            b.WithRecursiveDepth(3).WithTreeDepth(1).WithRepeatCount(1);
        });

        // close to equivalency required to reconcile precision differences between EF and Postgres
        AssertionOptions.AssertEquivalencyUsing(options =>
        {
            options
                .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1.Seconds()))
                .WhenTypeIs<DateTime>();
            options
                .Using<DateTimeOffset>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1.Seconds()))
                .WhenTypeIs<DateTimeOffset>();

            return options;
        });

        // new WireMockServer() is equivalent to call WireMockServer.Start()
        WireMockServer = WireMockServer.Start();
        WireMockServerUrl = WireMockServer.Url!;

        Factory = new CustomWebApplicationFactory<TEntryPoint>();
    }

    public async ValueTask InitializeAsync()
    {
        _messageSink.OnMessage(new DiagnosticMessage("SharedFixture Started..."));

        // Service provider will build after getting with get accessors, we don't want to build our service provider here
        await Factory.InitializeAsync();
        await PostgresContainerFixture.InitializeAsync();
        await MongoContainerFixture.InitializeAsync();
        await RabbitMqContainerFixture.InitializeAsync();
        await RedisContainerFixture.InitializeAsync();

        // or using `AddOverrideEnvKeyValues` and using `__` as seperator to change configs that are accessible during service registration with BindOptions
        // with `AddOverrideInMemoryConfig` config changes are accessible after services registration and build process through IOptions<> with ServiceProvider
        Factory.AddOverrideEnvKeyValues(keyValues =>
        {
            keyValues.Add(
                $"{nameof(PostgresOptions)}__{nameof(PostgresOptions.ConnectionString)}",
                PostgresContainerFixture.PostgresContainer.GetConnectionString()
            );

            keyValues.Add(
                $"{nameof(MessagePersistenceOptions)}__{nameof(PostgresOptions.ConnectionString)}",
                PostgresContainerFixture.PostgresContainer.GetConnectionString()
            );

            keyValues.Add(
                $"{nameof(MongoOptions)}__{nameof(MongoOptions.ConnectionString)}",
                MongoContainerFixture.ConnectionString
            );

            keyValues.Add(
                $"{nameof(MasstransitOptions)}__{nameof(MasstransitOptions.RabbitMQConnectionString)}",
                RabbitMqContainerFixture.Container.GetConnectionString()
            );

            keyValues.Add(
                $"{nameof(BuildingBlocks.Caching.CacheOptions)}__{nameof(RedisDistributedCacheOptions)}__{nameof(RedisDistributedCacheOptions.ConnectionString)}",
                RedisContainerFixture.Container.GetConnectionString()
            );
        });

        // with `AddOverrideInMemoryConfig` config changes are accessible after services registration and build process
        Factory.WithTestConfiguration(cfg =>
        {
            // Or we can override configuration explicitly, and it is accessible via IOptions<> and Configuration
            cfg["WireMockUrl"] = WireMockServerUrl;
        });

        if (SharedFixtureInitialized is not null)
        {
            await SharedFixtureInitialized.Invoke();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await PostgresContainerFixture.DisposeAsync();
        await MongoContainerFixture.DisposeAsync();
        await RabbitMqContainerFixture.DisposeAsync();
        await RedisContainerFixture.DisposeAsync();

        WireMockServer.Stop();
        AdminHttpClient.Dispose();
        NormalUserHttpClient.Dispose();
        GuestClient.Dispose();

        if (SharedFixtureDisposed is not null)
        {
            await SharedFixtureDisposed.Invoke();
        }

        await Factory.DisposeAsync();

        _messageSink.OnMessage(new DiagnosticMessage("SharedFixture Stopped..."));
    }

    public void WithTestConfigureServices(Action<IServiceCollection> services)
    {
        Factory.WithTestConfigureServices(services);
    }

    public void WithTestConfigureAppConfiguration(Action<WebHostBuilderContext, IConfigurationBuilder> appConfiguration)
    {
        Factory.WithTestConfigureAppConfiguration(appConfiguration);
    }

    public void WithTestConfiguration(Action<IConfiguration> configurations)
    {
        Factory.WithTestConfiguration(configurations);
    }

    public void AddOverrideEnvKeyValues(Action<IDictionary<string, string>> keyValuesAction)
    {
        Factory.AddOverrideEnvKeyValues(keyValuesAction);
    }

    public void AddOverrideInMemoryConfig(Action<IDictionary<string, string>> keyValuesAction)
    {
        Factory.AddOverrideInMemoryConfig(keyValuesAction);
    }

    public async Task CleanupAsync(CancellationToken cancellationToken = default)
    {
        await PostgresContainerFixture.ResetDbAsync(cancellationToken);
        await MongoContainerFixture.ResetDbAsync(cancellationToken);
        await RabbitMqContainerFixture.CleanupQueuesAsync(cancellationToken);
    }

    public void SetAdminUser()
    {
        var admin = SharedFixture<TEntryPoint>.CreateAdminUserMock();
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
        CancellationToken cancellationToken = default
    )
    {
        return await ExecuteScopeAsync(async sp =>
        {
            var mediator = sp.GetRequiredService<IMediator>();

            return await mediator.Send(request, cancellationToken);
        });
    }

    public async Task<TResponse> CommandAsync<TResponse>(
        BuildingBlocks.Abstractions.Commands.ICommand<TResponse> command,
        CancellationToken cancellationToken = default
    )
        where TResponse : class
    {
        return await ExecuteScopeAsync(async sp =>
        {
            var commandBus = sp.GetRequiredService<ICommandBus>();

            return await commandBus.SendAsync(command, cancellationToken);
        });
    }

    public async Task CommandAsync(ICommand command, CancellationToken cancellationToken = default)
    {
        await ExecuteScopeAsync(async sp =>
        {
            var commandBus = sp.GetRequiredService<ICommandBus>();

            await commandBus.SendAsync(command, cancellationToken);
        });
    }

    public async Task<TResponse> QueryAsync<TResponse>(
        BuildingBlocks.Abstractions.Queries.IQuery<TResponse> query,
        CancellationToken cancellationToken = default
    )
        where TResponse : class
    {
        return await ExecuteScopeAsync(async sp =>
        {
            var queryProcessor = sp.GetRequiredService<IQueryBus>();

            return await queryProcessor.SendAsync(query, cancellationToken);
        });
    }

    public async ValueTask PublishMessageAsync<TMessage>(
        TMessage message,
        CancellationToken cancellationToken = default
    )
        where TMessage : class, IMessage
    {
        await ExecuteScopeAsync(async sp =>
        {
            var bus = sp.GetRequiredService<IExternalEventBus>();

            await bus.PublishAsync(message, cancellationToken);
        });
    }

    public async ValueTask PublishMessageAsync<TMessage>(
        IMessageEnvelope<TMessage> messageEnvelope,
        CancellationToken cancellationToken = default
    )
        where TMessage : class, IMessage
    {
        await ExecuteScopeAsync(async sp =>
        {
            var bus = sp.GetRequiredService<IExternalEventBus>();

            await bus.PublishAsync(messageEnvelope, cancellationToken);
        });
    }

    // Ref: https://tech.energyhelpline.com/in-memory-testing-with-masstransit/
    public async ValueTask WaitUntilConditionMet(
        Func<Task<bool>> conditionToMet,
        int? timeoutSecond = null,
        string? exception = null
    )
    {
        var time = timeoutSecond ?? 300;

        var startTime = DateTime.Now;
        var timeoutExpired = false;
        var meet = await conditionToMet.Invoke();
        while (!meet)
        {
            if (timeoutExpired)
            {
                throw new TimeoutException(
                    exception ?? $"Condition not met for the test in the '{timeoutExpired}' second."
                );
            }

            await Task.Delay(100);
            meet = await conditionToMet.Invoke();
            timeoutExpired = DateTime.Now - startTime > TimeSpan.FromSeconds(time);
        }
    }

    public async Task ShouldPublishing<T>(CancellationToken cancellationToken = default)
        where T : class, IMessage
    {
        // will block the thread until there is a publishing message
        await MasstransitHarness.Published.Any(
            message =>
            {
                var messageFilter = new PublishedMessageFilter();
                var faultMessageFilter = new PublishedMessageFilter();

                messageFilter.Includes.Add<T>();
                messageFilter.Includes.Add<IMessageEnvelope<T>>();
                faultMessageFilter.Includes.Add<Fault<IMessageEnvelope<T>>>();
                faultMessageFilter.Includes.Add<T>();

                var faulty = faultMessageFilter.Any(message);
                var published = messageFilter.Any(message);

                return published & !faulty;
            },
            cancellationToken
        );
    }

    public async Task ShouldSending<T>(CancellationToken cancellationToken = default)
        where T : class, IMessage
    {
        // will block the thread until there is a publishing message
        await MasstransitHarness.Sent.Any(
            message =>
            {
                var messageFilter = new SentMessageFilter();
                var faultMessageFilter = new SentMessageFilter();

                messageFilter.Includes.Add<T>();
                messageFilter.Includes.Add<IMessageEnvelope<T>>();
                faultMessageFilter.Includes.Add<Fault<IMessageEnvelope<T>>>();
                faultMessageFilter.Includes.Add<Fault<T>>();

                var faulty = faultMessageFilter.Any(message);
                var published = messageFilter.Any(message);

                return published & !faulty;
            },
            cancellationToken
        );
    }

    public async Task ShouldConsuming<T>(CancellationToken cancellationToken = default)
        where T : class, IMessage
    {
        // will block the thread until there is a consuming message
        await MasstransitHarness.Consumed.Any(
            message =>
            {
                var messageFilter = new ReceivedMessageFilter();
                var faultMessageFilter = new ReceivedMessageFilter();

                messageFilter.Includes.Add<IMessageEnvelope<T>>();
                messageFilter.Includes.Add<T>();

                faultMessageFilter.Includes.Add<Fault<IMessageEnvelope<T>>>();
                faultMessageFilter.Includes.Add<Fault<T>>();

                var faulty = faultMessageFilter.Any(message);
                var published = messageFilter.Any(message);

                return published & !faulty;
            },
            cancellationToken
        );
    }

    public async Task ShouldConsuming<TMessage, TConsumedBy>(CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
        where TConsumedBy : class, IConsumer
    {
        var consumerHarness = ServiceProvider.GetRequiredService<IConsumerTestHarness<TConsumedBy>>();

        // will block the thread until there is a consuming message
        await consumerHarness.Consumed.Any(
            message =>
            {
                var messageFilter = new ReceivedMessageFilter();
                var faultMessageFilter = new ReceivedMessageFilter();

                messageFilter.Includes.Add<IMessageEnvelope<TMessage>>();
                messageFilter.Includes.Add<TMessage>();

                faultMessageFilter.Includes.Add<Fault<TMessage>>();
                faultMessageFilter.Includes.Add<Fault<IMessageEnvelope<TMessage>>>();

                var faulty = faultMessageFilter.Any(message);
                var published = messageFilter.Any(message);

                return published & !faulty;
            },
            cancellationToken
        );
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

    public async ValueTask ShouldProcessingOutboxMessage<TMessage>(CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
    {
        await WaitUntilConditionMet(async () =>
        {
            return await ExecuteScopeAsync(async sp =>
            {
                var messagePersistenceService = sp.GetService<IMessagePersistenceService>();
                messagePersistenceService.NotBeNull();

                var filter = await messagePersistenceService.GetByFilterAsync(
                    x =>
                        x.DeliveryType == MessageDeliveryType.Outbox
                        && TypeMapper.AddFullTypeName(typeof(TMessage)) == x.DataType,
                    cancellationToken
                );

                var res = filter.Any(x => x.MessageStatus == MessageStatus.Delivered);

                return res;
            });
        });
    }

    public async ValueTask ShouldProcessingInternalCommand<TInternalCommand>(
        CancellationToken cancellationToken = default
    )
        where TInternalCommand : class, IInternalCommand
    {
        await WaitUntilConditionMet(async () =>
        {
            return await ExecuteScopeAsync(async sp =>
            {
                var messagePersistenceService = sp.GetService<IMessagePersistenceService>();
                messagePersistenceService.NotBeNull();

                var filter = await messagePersistenceService.GetByFilterAsync(
                    x =>
                        x.DeliveryType == MessageDeliveryType.Internal
                        && TypeMapper.AddFullTypeName(typeof(TInternalCommand)) == x.DataType,
                    cancellationToken
                );

                var res = filter.Any(x => x.MessageStatus == MessageStatus.Delivered);

                return res;
            });
        });
    }

    private HttpClient CreateAdminHttpClient()
    {
        var adminClient = Factory.CreateClient();

        // Set the media type of the request to JSON - we need this for getting problem details result for all http calls because problem details just return response for request with media type JSON
        adminClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //https://github.com/webmotions/fake-authentication-jwtbearer/issues/14
        var claims = CreateAdminUserMock().Claims;

        adminClient.SetFakeJwtBearerClaims(claims);

        return adminClient;
    }

    private HttpClient CreateNormalUserHttpClient()
    {
        var userClient = Factory.CreateClient();

        // Set the media type of the request to JSON - we need this for getting problem details result for all http calls because problem details just return response for request with media type JSON
        userClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //https://github.com/webmotions/fake-authentication-jwtbearer/issues/14
        var claims = CreateNormalUserMock().Claims;

        userClient.SetFakeJwtBearerClaims(claims);

        return userClient;
    }

    private static MockAuthUser CreateAdminUserMock()
    {
        var roleClaim = new Claim(ClaimTypes.Role, Constants.Users.Admin.Role);
        var otherClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Constants.Users.Admin.UserId),
            new(ClaimTypes.Name, Constants.Users.Admin.UserName),
            new(ClaimTypes.Email, Constants.Users.Admin.Email),
        };

        return _ = new MockAuthUser(otherClaims.Concat([roleClaim]).ToArray());
    }

    private static MockAuthUser CreateNormalUserMock()
    {
        var roleClaim = new Claim(ClaimTypes.Role, Constants.Users.NormalUser.Role);
        var otherClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Constants.Users.NormalUser.UserId),
            new(ClaimTypes.Name, Constants.Users.NormalUser.UserName),
            new(ClaimTypes.Email, Constants.Users.NormalUser.Email),
        };

        return _ = new MockAuthUser(otherClaims.Concat([roleClaim]).ToArray());
    }
}
