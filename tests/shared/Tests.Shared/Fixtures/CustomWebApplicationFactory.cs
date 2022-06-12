using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Core.Messaging.BackgroundServices;
using BuildingBlocks.Core.Utils;
using MassTransit;
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
using Serilog;
using Serilog.Events;
using Tests.Shared.Extensions;
using Tests.Shared.Mocks;
using Xunit.Abstractions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Tests.Shared.Fixtures;

public class CustomWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>
    where TEntryPoint : class
{
    public IConfiguration Configuration => Services.GetRequiredService<IConfiguration>();
    public ITestOutputHelper? OutputHelper { get; set; }
    public Action<IServiceCollection>? TestRegistrationServices { get; set; }

    public ILogger Logger => Services.GetRequiredService<ILogger<CustomWebApplicationFactory<TEntryPoint>>>();
    public void ClearOutputHelper() => OutputHelper = null;
    public void SetOutputHelper(ITestOutputHelper value) => OutputHelper = value;

    public CustomWebApplicationFactory(Action<IServiceCollection>? testRegistrationServices = null)
    {
        TestRegistrationServices = testRegistrationServices ?? (collection => { });
    }

    // https://andrewlock.net/converting-integration-tests-to-net-core-3/
    // https://andrewlock.net/exploring-dotnet-6-part-6-supporting-integration-tests-with-webapplicationfactory-in-dotnet-6/
    // https://github.com/dotnet/aspnetcore/pull/33462
    // https://github.com/dotnet/aspnetcore/issues/33846
    // https://milestone.topics.it/2021/04/28/you-wanna-test-http.html
    // https://timdeschryver.dev/blog/refactor-functional-tests-to-support-minimal-web-apis
    // https://timdeschryver.dev/blog/how-to-test-your-csharp-web-api
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseContentRoot(Directory.GetCurrentDirectory());

        builder.UseSerilog((ctx, loggerConfiguration) =>
        {
            //https://github.com/jhquirino/Serilog.Sinks.Xunit2
            if (OutputHelper is { })
            {
                loggerConfiguration.WriteTo.Xunit(OutputHelper);
            }

            loggerConfiguration.MinimumLevel.Is(LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning);
        });

        // builder.ConfigureLogging(logging =>
        // {
        //     //https://github.com/serilog/serilog-aspnetcore/issues/57#issuecomment-407569450
        //     logging.ClearProviders(); // Remove other loggers
        //
        //     // https://github.com/martincostello/xunit-logging
        //     if (OutputHelper is { })
        //         logging.AddXUnit(OutputHelper); // Use the ITestOutputHelper instance
        // });

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

        //The test app's builder.ConfigureTestServices callback is executed after the app's Startup.ConfigureServices code is executed.
        builder.ConfigureTestServices(services =>
        {
            // services.RemoveAll(typeof(IHostedService));

            // will execute automatically by .net core
            // services.AddHostedService<MassTransitHostedService>();

            var descriptor = services.Single(s => s.ImplementationType == typeof(MessagePersistenceBackgroundService));
            services.Remove(descriptor);

            services.AddScoped<MessagePersistenceBackgroundService>();

            services.AddScoped(_ => CreateAnonymouslyUserMock());

            // https://milestone.topics.it/2021/11/10/http-client-factory-in-integration-testing.html
            services.Replace(new ServiceDescriptor(typeof(IHttpClientFactory),
                new DelegateHttpClientFactory(ClientProvider)));

            // This helper just supports jwt Scheme, and for Identity server Scheme will crash so we should disable AddIdentityServer()
            services.ReplaceSingleton<IHttpContextAccessor>(CreateHttpContextAccessorMock);
            services.AddTestAuthentication();

            TestRegistrationServices?.Invoke(services);
        });

        builder.UseDefaultServiceProvider((env, c) =>
        {
            // Handling Captive Dependency Problem
            // https://ankitvijay.net/2020/03/17/net-core-and-di-beware-of-captive-dependency/
            // https://blog.ploeh.dk/2014/06/02/captive-dependency/
            if (env.HostingEnvironment.IsEnvironment("test") || env.HostingEnvironment.IsDevelopment())
                c.ValidateScopes = true;
        });
    }

    public Lazy<HttpClient> ClientProvider(string name)
    {
        return new Lazy<HttpClient>(() => CreateDefaultClient());
    }

    private static IHttpContextAccessor CreateHttpContextAccessorMock(IServiceProvider serviceProvider)
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
}
