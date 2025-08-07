using BuildingBlocks.Abstractions.Persistence;
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
using WebMotions.Fake.Authentication.JwtBearer;
using Xunit;
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
    private readonly Dictionary<string, string?> _inMemoryConfigs = [];
    private readonly List<string> _overrideEnvKeysToDispose = [];
    private Action<IServiceCollection>? _testConfigureServices;
    private Action<IConfiguration>? _testConfiguration;
    private Action<WebHostBuilderContext, IConfigurationBuilder>? _testConfigureAppConfiguration;
    private readonly List<Type> _testHostedServicesTypes = new();
    private string _environment = Environments.Test;

    /// <summary>
    /// Use for tracking occured log events for testing purposes
    /// </summary>
    public InMemoryLoggerProvider InMemoryLogTrackerProvider { get; } = new();

    public CustomWebApplicationFactory<TEntryPoint> WithTestConfigureServices(Action<IServiceCollection> services)
    {
        _testConfigureServices += services;
        return this;
    }

    public CustomWebApplicationFactory<TEntryPoint> WithTestConfiguration(Action<IConfiguration> configurations)
    {
        _testConfiguration += configurations;
        return this;
    }

    public CustomWebApplicationFactory<TEntryPoint> WithTestConfigureAppConfiguration(
        Action<WebHostBuilderContext, IConfigurationBuilder> appConfigurations
    )
    {
        _testConfigureAppConfiguration += appConfigurations;
        return this;
    }

    public CustomWebApplicationFactory<TEntryPoint> WithEnvironment(string environment)
    {
        _environment = environment;
        return this;
    }

    public CustomWebApplicationFactory<TEntryPoint> AddTestHostedService<THostedService>()
        where THostedService : class, IHostedService
    {
        _testHostedServicesTypes.Add(typeof(THostedService));

        return this;
    }

    public ILogger Logger => Services.GetRequiredService<ILogger<CustomWebApplicationFactory<TEntryPoint>>>();

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
        builder.UseEnvironment(_environment);
        builder.UseContentRoot(".");

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
                //// add in-memory configuration instead of using appestings.json and override existing settings, and it is accessible via IOptions and Configuration
                //// https://blog.markvincze.com/overriding-configuration-in-asp-net-core-integration-tests/
                configurationBuilder.AddInMemoryCollection(_inMemoryConfigs);

                _testConfiguration?.Invoke(hostingContext.Configuration);
                _testConfigureAppConfiguration?.Invoke(hostingContext, configurationBuilder);
            }
        );

        builder.ConfigureTestServices(services =>
        {
            // https://andrewlock.net/converting-integration-tests-to-net-core-3/
            // add test-hosted services
            foreach (var hostedServiceType in _testHostedServicesTypes)
            {
                services.AddSingleton(typeof(IHostedService), hostedServiceType);
            }

            // TODO: Web could use this in E2E test for running another service during our test
            // https://milestone.topics.it/2021/11/10/http-client-factory-in-integration-testing.html

            // add authentication using a fake jwt bearer - we can use SetAdminUser method to set authenticate user to existing HttContextAccessor
            // https://blog.joaograssi.com/posts/2021/asp-net-core-testing-permission-protected-api-endpoints/
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
                .Services.AddCustomJwtAuthorization(
                    rolePolicies: new List<RolePolicy>
                    {
                        new(Constants.Users.Admin.Role, new List<string> { Constants.Users.Admin.Role }),
                        new(Constants.Users.NormalUser.Role, new List<string> { Constants.Users.NormalUser.Role }),
                    },
                    scheme: FakeJwtBearerDefaults.AuthenticationScheme
                );

            // Add your test delegating handler
            services.AddTransient<TestHeaderPropagationHandler>();
            // Apply it to all HttpClient instances
            services.ConfigureHttpClientDefaults(httpClientBuilder =>
                httpClientBuilder.AddHttpMessageHandler<TestHeaderPropagationHandler>()
            );

            var referencingAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            services.Scan(scan =>
                scan.FromAssemblies(referencingAssemblies)
                    .AddClasses(classes => classes.AssignableTo<ITestDataSeeder>())
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            );

            _testConfigureServices?.Invoke(services);
        });

        // override Configure is not valid in test: //https://github.com/dotnet/aspnetcore/issues/45372
        // wb.Configure(x =>
        // {
        // });

        base.ConfigureWebHost(builder);
    }

    public CustomWebApplicationFactory<TEntryPoint> AddOverrideInMemoryConfig(
        Action<IDictionary<string, string>> inmemoryConfigsAction
    )
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

        return this;
    }

    public CustomWebApplicationFactory<TEntryPoint> AddOverrideEnvKeyValues(
        Action<IDictionary<string, string>> keyValuesAction
    )
    {
        var keyValues = new Dictionary<string, string>();
        keyValuesAction.Invoke(keyValues);

        foreach (var (key, value) in keyValues)
        {
            _overrideEnvKeysToDispose.Add(key);
            // overriding app configs with using environments
            Environment.SetEnvironmentVariable(key, value);
        }

        return this;
    }

    public ValueTask InitializeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public new async Task DisposeAsync()
    {
        CleanupOverrideEnvKeys();

        await base.DisposeAsync();
    }

    private void CleanupOverrideEnvKeys()
    {
        foreach (string disposeEnvKey in _overrideEnvKeysToDispose)
        {
            Environment.SetEnvironmentVariable(disposeEnvKey, null);
        }
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
