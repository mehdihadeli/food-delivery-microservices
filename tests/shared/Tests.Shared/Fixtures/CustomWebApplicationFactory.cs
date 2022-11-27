using BuildingBlocks.Core.Messaging.BackgroundServices;
using BuildingBlocks.Web.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Tests.Shared.Auth;
using WebMotions.Fake.Authentication.JwtBearer;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Tests.Shared.Fixtures;

// https://bartwullems.blogspot.com/2022/01/net-6-minimal-apiintegration-testing.html
// https://milestone.topics.it/2021/04/28/you-wanna-test-http.html
public class CustomWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>, IAsyncLifetime
    where TEntryPoint : class
{
    private ITestOutputHelper? _outputHelper;

    private Action<IWebHostBuilder>? _customWebHostBuilder;
    private Action<IHostBuilder>? _customHostBuilder;
    private Action<WebHostBuilderContext, IConfigurationBuilder>? _configureAppConfigurations;

    public IConfiguration Configuration => Services.GetRequiredService<IConfiguration>();
    public Action<IServiceCollection>? TestConfigureServices { get; set; }
    public Action<IApplicationBuilder>? TestConfigureApp { get; set; }

    public ILogger Logger => Services.GetRequiredService<ILogger<CustomWebApplicationFactory<TEntryPoint>>>();
    public void ClearOutputHelper() => _outputHelper = null;
    public void SetOutputHelper(ITestOutputHelper value) => _outputHelper = value;

    public CustomWebApplicationFactory()
    {
    }

    //https://github.com/xunit/xunit/issues/1568
    private CustomWebApplicationFactory(
        Action<IServiceCollection> configureServices,
        Action<IApplicationBuilder> configureApp):base()
    {
        TestConfigureServices += configureServices;
        TestConfigureApp += configureApp;
    }

    public static CustomWebApplicationFactory<TEntryPoint> Configure(
        Action<IServiceCollection> configureServices,
        Action<IApplicationBuilder> configureApp)
        => new(configureServices, configureApp);

    public CustomWebApplicationFactory<TEntryPoint> WithConfigureAppConfigurations(
        Action<WebHostBuilderContext, IConfigurationBuilder> builder)
    {
        _configureAppConfigurations += builder;

        return this;
    }

    public new CustomWebApplicationFactory<TEntryPoint> WithWebHostBuilder(Action<IWebHostBuilder> builder)
    {
        _customWebHostBuilder = builder;

        return this;
    }

    public CustomWebApplicationFactory<TEntryPoint> WithHostBuilder(Action<IHostBuilder> builder)
    {
        _customHostBuilder = builder;

        return this;
    }

    // https://github.com/davidfowl/TodoApi/
    // https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-7.0
    // https://andrewlock.net/converting-integration-tests-to-net-core-3/
    // https://andrewlock.net/exploring-dotnet-6-part-6-supporting-integration-tests-with-webapplicationfactory-in-dotnet-6/
    // https://github.com/dotnet/aspnetcore/pull/33462
    // https://github.com/dotnet/aspnetcore/issues/33846
    // https://milestone.topics.it/2021/04/28/you-wanna-test-http.html
    // https://timdeschryver.dev/blog/refactor-functional-tests-to-support-minimal-web-apis
    // https://timdeschryver.dev/blog/how-to-test-your-csharp-web-api
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // //https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/
        // //https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration-providers#json-configuration-provider
        builder.ConfigureAppConfiguration((hostingContext, configurationBuilder) =>
        {
            // configurationBuilder.Sources.Clear();
            // IHostEnvironment env = hostingContext.HostingEnvironment;
            //
            // configurationBuilder
            //     .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            //     .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
            //     .AddJsonFile("integrationappsettings.json", true, true);
            //
            // var integrationConfig = configurationBuilder.Build();
            //
            // configurationBuilder.AddConfiguration(integrationConfig);

            //// add in-memory configuration instead of using appestings.json and override existing settings and it is accessible via IOptions and Configuration
            //// https://blog.markvincze.com/overriding-configuration-in-asp-net-core-integration-tests/
            // configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?> {});

            _configureAppConfigurations?.Invoke(hostingContext, configurationBuilder);
        });

        //https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests#set-the-environment
        //https://stackoverflow.com/questions/43927955/should-getenvironmentvariable-work-in-xunit-test/43951218

        //we could read env from our test launch setting or we can set it directly here
        builder.UseEnvironment("test");

        //The test app's builder.ConfigureServices callback is executed before the SUT's Startup.ConfigureServices code.
        builder.ConfigureServices(services =>
        {
            services.AddScoped<TextWriter>(_ => new StringWriter());
            services.AddScoped<TextReader>(sp =>
                new StringReader(sp.GetRequiredService<TextWriter>().ToString() ?? ""));
        });

        // Is be called after the `ConfigureServices` from the Startup, which allows you to overwrite the DI with mocked instances
        builder.ConfigureTestServices(services =>
        {
            // services.RemoveAll(typeof(IHostedService));

            // will add automatically with AddMassTransitTestHarness and AddMassTransit
            // services.AddHostedService<MassTransitHostedService>();

            var descriptor = services.Single(s => s.ImplementationType == typeof(MessagePersistenceBackgroundService));
            services.Remove(descriptor);

            services.AddScoped<MessagePersistenceBackgroundService>();

            // TODO: Web could use this in E2E test for running another service during our test
            // https://milestone.topics.it/2021/11/10/http-client-factory-in-integration-testing.html
            // services.Replace(new ServiceDescriptor(typeof(IHttpClientFactory),
            //     new DelegateHttpClientFactory(ClientProvider)));

            // // This helper just supports jwt Scheme, and for Identity server Scheme will crash so we should disable AddIdentityServer()
            // services.AddScoped(_ => CreateAnonymouslyUserMock());
            // services.ReplaceSingleton(CreateCustomTestHttpContextAccessorMock);
            // services.AddTestAuthentication();

            // Or
            // add authentication using a fake jwt bearer - we can use SetAdminUser method to set authenticate user to existing HttContextAccessor
            // https://github.com/webmotions/fake-authentication-jwtbearer
            // https://github.com/webmotions/fake-authentication-jwtbearer/issues/14
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = FakeJwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = FakeJwtBearerDefaults.AuthenticationScheme;
            }).AddFakeJwtBearer();

            TestConfigureServices?.Invoke(services);
        });

        builder.Configure(appBuilder =>
        {
            TestConfigureApp?.Invoke(appBuilder);
        });

        builder.UseDefaultServiceProvider((env, c) =>
        {
            // Handling Captive Dependency Problem
            // https://ankitvijay.net/2020/03/17/net-core-and-di-beware-of-captive-dependency/
            // https://blog.ploeh.dk/2014/06/02/captive-dependency/
            if (env.HostingEnvironment.IsTest() || env.HostingEnvironment.IsDevelopment())
                c.ValidateScopes = true;
        });

        _customWebHostBuilder?.Invoke(builder);
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // UseSerilog on WebHostBuilder is absolute so we should use IHostBuilder
        builder.UseSerilog((ctx, loggerConfiguration) =>
        {
            //https://github.com/jhquirino/Serilog.Sinks.Xunit2
            if (_outputHelper is { })
            {
                loggerConfiguration.WriteTo.Xunit(_outputHelper);
            }

            loggerConfiguration.MinimumLevel.Is(LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning);
        });

        _customHostBuilder?.Invoke(builder);

        return base.CreateHost(builder);
    }

    private static IHttpContextAccessor CreateCustomTestHttpContextAccessorMock(IServiceProvider serviceProvider)
    {
        var httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
        using var scope = serviceProvider.CreateScope();
        httpContextAccessorMock.HttpContext = new DefaultHttpContext {RequestServices = scope.ServiceProvider,};

        httpContextAccessorMock.HttpContext.Request.Host = new HostString("localhost", 5000);
        httpContextAccessorMock.HttpContext.Request.Scheme = "http";
        var res = httpContextAccessorMock.HttpContext.AuthenticateAsync(Constants.AuthConstants.Scheme).GetAwaiter()
            .GetResult();
        httpContextAccessorMock.HttpContext.User = res.Ticket?.Principal!;
        return httpContextAccessorMock;
    }

    private MockAuthUser CreateAnonymouslyUserMock()
    {
        return new MockAuthUser();
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }
}
