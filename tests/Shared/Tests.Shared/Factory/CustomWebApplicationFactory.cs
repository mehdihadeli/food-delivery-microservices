using BuildingBlocks.Core.Web.Extensions;
using BuildingBlocks.Security.Jwt;
using Meziantou.Extensions.Logging.InMemory;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using WebMotions.Fake.Authentication.JwtBearer;
using Environments = BuildingBlocks.Core.Web.Environments;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Tests.Shared.Factory;

// https://bartwullems.blogspot.com/2022/01/net-6-minimal-apiintegration-testing.html
// https://milestone.topics.it/2021/04/28/you-wanna-test-http.html
public class CustomWebApplicationFactory<TEntryPoint>(Action<IWebHostBuilder>? webHostBuilder = null)
    : WebApplicationFactory<TEntryPoint>,
        IAsyncLifetime
    where TEntryPoint : class
{
    private ITestOutputHelper? _outputHelper;
    private readonly Dictionary<string, string?> _inMemoryConfigs = new();
    private Action<IServiceCollection>? _testConfigureServices;
    private Action<IConfiguration>? _testConfiguration;
    private Action<WebHostBuilderContext, IConfigurationBuilder>? _testConfigureAppConfiguration;
    private readonly List<Type> _testHostedServicesTypes = new();

    /// <summary>
    /// Use for tracking occured log events for testing purposes
    /// </summary>
    public InMemoryLoggerProvider InMemoryLogTrackerProvider { get; } = new();

    public void WithTestConfigureServices(Action<IServiceCollection> services)
    {
        _testConfigureServices += services;
    }

    public void WithTestConfiguration(Action<IConfiguration> configurations)
    {
        _testConfiguration += configurations;
    }

    public void WithTestConfigureAppConfiguration(
        Action<WebHostBuilderContext, IConfigurationBuilder> appConfigurations
    )
    {
        _testConfigureAppConfiguration += appConfigurations;
    }

    public void AddTestHostedService<THostedService>()
        where THostedService : class, IHostedService
    {
        _testHostedServicesTypes.Add(typeof(THostedService));
    }

    public ILogger Logger => Services.GetRequiredService<ILogger<CustomWebApplicationFactory<TEntryPoint>>>();

    public void ClearOutputHelper() => _outputHelper = null;

    public void SetOutputHelper(ITestOutputHelper value) => _outputHelper = value;

    // https://github.com/davidfowl/TodoApi/
    // https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests
    // https://andrewlock.net/converting-integration-tests-to-net-core-3/
    // https://andrewlock.net/exploring-dotnet-6-part-6-supporting-integration-tests-with-webapplicationfactory-in-dotnet-6/
    // https://github.com/dotnet/aspnetcore/pull/33462
    // https://github.com/dotnet/aspnetcore/issues/33846
    // https://milestone.topics.it/2021/04/28/you-wanna-test-http.html
    // https://timdeschryver.dev/blog/refactor-functional-tests-to-support-minimal-web-apis
    // https://timdeschryver.dev/blog/how-to-test-your-csharp-web-api
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Test);
        builder.UseContentRoot(".");

        // UseSerilog on WebHostBuilder is absolute so we should use IHostBuilder
        builder.UseSerilog(
            (ctx, loggerConfiguration) =>
            {
                // https://www.meziantou.net/how-to-test-the-logs-from-ilogger-in-dotnet.htm
                // We could also create a serilog sink for this in-memoryLoggerProvider for keep-tracking logs in the test and their states
                var loggerProviderCollections = new LoggerProviderCollection();
                loggerProviderCollections.AddProvider(InMemoryLogTrackerProvider);
                loggerConfiguration.WriteTo.Providers(loggerProviderCollections);

                //https://github.com/trbenning/serilog-sinks-xunit
                if (_outputHelper is { })
                {
                    loggerConfiguration.WriteTo.TestOutput(
                        _outputHelper,
                        LogEventLevel.Information,
                        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level} - {Message:lj}{NewLine}{Exception}"
                    );
                }
            }
        );

        builder.UseDefaultServiceProvider(
            (env, c) =>
            {
                // Handling Captive Dependency Problem
                // https://ankitvijay.net/2020/03/17/net-core-and-di-beware-of-captive-dependency/
                // https://blog.ploeh.dk/2014/06/02/captive-dependency/
                if (env.HostingEnvironment.IsTest() || env.HostingEnvironment.IsDevelopment())
                    c.ValidateScopes = true;
            }
        );

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        webHostBuilder?.Invoke(builder);

        builder.ConfigureAppConfiguration(
            (hostingContext, configurationBuilder) =>
            {
                //// add in-memory configuration instead of using appestings.json and override existing settings and it is accessible via IOptions and Configuration
                //// https://blog.markvincze.com/overriding-configuration-in-asp-net-core-integration-tests/
                configurationBuilder.AddInMemoryCollection(_inMemoryConfigs);

                _testConfiguration?.Invoke(hostingContext.Configuration);
                _testConfigureAppConfiguration?.Invoke(hostingContext, configurationBuilder);
            }
        );

        builder.ConfigureTestServices(services =>
        {
            //// https://andrewlock.net/converting-integration-tests-to-net-core-3/
            //// 1. If we need fine-grained control over the behavior of the services during tests, we can remove all IHostedService in CustomWebApplicationFactory for existing app and add required ones with `AddTestHostedService` in SharedFixture `InitializeAsync` and run them manually with `TestWorkersRunner`.
            //// 2. We can use Existing IHostedService Implementations if we want our tests to be as realistic as possible.
            // services.RemoveAll<IHostedService>();
            // // add test hosted services
            // foreach (var hostedServiceType in _testHostedServicesTypes)
            // {
            //     services.AddSingleton(typeof(IHostedService), hostedServiceType);
            // }

            // TODO: Web could use this in E2E test for running another service during our test
            // https://milestone.topics.it/2021/11/10/http-client-factory-in-integration-testing.html
            // services.Replace(new ServiceDescriptor(typeof(IHttpClientFactory),
            //     new DelegateHttpClientFactory(ClientProvider)));

            //// https://blog.joaograssi.com/posts/2021/asp-net-core-testing-permission-protected-api-endpoints/
            //// This helper just supports jwt Scheme, and for Identity server Scheme will crash so we should disable AddIdentityServer()
            // services.TryAddScoped(_ => CreateAnonymouslyUserMock());
            // services.ReplaceSingleton(CreateCustomTestHttpContextAccessorMock);
            // services.AddTestAuthentication();

            // Or
            // add authentication using a fake jwt bearer - we can use SetAdminUser method to set authenticate user to existing HttContextAccessor
            // https://github.com/webmotions/fake-authentication-jwtbearer
            // https://github.com/webmotions/fake-authentication-jwtbearer/issues/14
            services
                // will skip registering dependencies if exists previously, but will override authentication option inner configure delegate through Configure<AuthenticationOptions>
                .AddAuthentication(options =>
                {
                    // choosing `FakeBearer` scheme (instead of exiting default scheme of application) as default in runtime for authentication and authorization middleware
                    options.DefaultAuthenticateScheme = FakeJwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = FakeJwtBearerDefaults.AuthenticationScheme;
                })
                .AddFakeJwtBearer(c =>
                {
                    // for working fake token this should be set to jwt
                    c.BearerValueType = FakeJwtBearerBearerValueType.Jwt;
                })
                .Services.AddCustomAuthorization(
                    rolePolicies: new List<RolePolicy>
                    {
                        new(Constants.Users.Admin.Role, new List<string> { Constants.Users.Admin.Role }),
                        new(Constants.Users.NormalUser.Role, new List<string> { Constants.Users.NormalUser.Role }),
                    },
                    scheme: FakeJwtBearerDefaults.AuthenticationScheme
                );

            _testConfigureServices?.Invoke(services);
        });

        // override Configure is not valid in test: //https://github.com/dotnet/aspnetcore/issues/45372
        // wb.Configure(x =>
        // {
        // });

        base.ConfigureWebHost(builder);
    }

    public void AddOverrideInMemoryConfig(Action<IDictionary<string, string>> inmemoryConfigsAction)
    {
        var inmemoryConfigs = new Dictionary<string, string>();
        inmemoryConfigsAction.Invoke(inmemoryConfigs);

        // overriding app configs with using in-memory configs
        // add in-memory configuration instead of using appestings.json and override existing settings and it is accessible via IOptions and Configuration
        // https://blog.markvincze.com/overriding-configuration-in-asp-net-core-integration-tests/
        foreach (var inmemoryConfig in inmemoryConfigs)
        {
            // Use `TryAdd` for prevent adding repetitive elements because of using IntegrationTestBase
            _inMemoryConfigs.TryAdd(inmemoryConfig.Key, inmemoryConfig.Value);
        }
    }

    public void AddOverrideEnvKeyValues(Action<IDictionary<string, string>> keyValuesAction)
    {
        var keyValues = new Dictionary<string, string>();
        keyValuesAction.Invoke(keyValues);

        foreach (var (key, value) in keyValues)
        {
            // overriding app configs with using environments
            Environment.SetEnvironmentVariable(key, value);
        }
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }

    private static IHttpContextAccessor CreateCustomTestHttpContextAccessorMock(IServiceProvider serviceProvider)
    {
        var httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
        using var scope = serviceProvider.CreateScope();
        httpContextAccessorMock.HttpContext = new DefaultHttpContext { RequestServices = scope.ServiceProvider };

        httpContextAccessorMock.HttpContext.Request.Host = new HostString("localhost", 5000);
        httpContextAccessorMock.HttpContext.Request.Scheme = "http";
        var res = httpContextAccessorMock
            .HttpContext.AuthenticateAsync(Constants.AuthConstants.Scheme)
            .GetAwaiter()
            .GetResult();
        httpContextAccessorMock.HttpContext.User = res.Ticket?.Principal!;
        return httpContextAccessorMock;
    }
}
