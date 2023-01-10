using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tests.Shared.Extensions;

namespace Tests.Shared.Factory;

/// <summary>
/// This WebApplicationFactory only use for testing web components without needing Program entrypoint and it doesn't work for web app with Program file and for this case we should use original WebApplicationFactory.
/// </summary>
/// <typeparam name="TEntryPoint"></typeparam>
class WebApplicationFactoryWithHost<TEntryPoint> :
    WebApplicationFactory<TEntryPoint>
    where TEntryPoint : class
{
    private readonly Action<IServiceCollection> _configureServices;
    readonly Action<IApplicationBuilder> _configure;
    readonly string[] _args;

    public ITestOutputHelper? TestOutputHelper { get; set; }
    public Action<IHostBuilder>? HostBuilderCustomization { get; set; }
    public Action<IWebHostBuilder>? WebHostBuilderCustomization { get; set; }

    public WebApplicationFactoryWithHost(
        Action<IServiceCollection> configureServices,
        Action<IApplicationBuilder> configure,
        string[]? args = null)
    {
        _configureServices = configureServices;
        _configure = configure;
        _args = args ?? Array.Empty<string>();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // change existing services ...
            if (TestOutputHelper != null)
                services.AddLogging(b => b.AddXUnit(TestOutputHelper));
        });

        builder.ConfigureTestServices(services =>
        {
            // change existing services ...
        });

        //https://github.com/dotnet/aspnetcore/issues/45319
        builder.Configure(app =>
        {
            //https://github.com/dotnet/aspnetcore/issues/37680#issuecomment-1331559463
            //https://github.com/dotnet/aspnetcore/issues/45319#issuecomment-1334355103
            //calling test configure setup first and then setup other configuration
            app.AddTestApplicationBuilder();

            // change application builder
        });
    }

    // This creates a new host, when there is no program file (EntryPoint) for finding the CreateDefaultBuilder - this approach use for testing web components without startup or program
    protected override IHostBuilder CreateHostBuilder()
    {
        var hostBuilder = Host.CreateDefaultBuilder(_args);
        // create startup with these configs
        hostBuilder.ConfigureWebHostDefaults((webBuilder) =>
        {
            webBuilder.ConfigureServices(_configureServices);

            //https://github.com/dotnet/aspnetcore/issues/37680#issuecomment-1331559463
            //https://github.com/dotnet/aspnetcore/issues/45319#issuecomment-1334355103
            // Set this so that the async context flows
            _configure.ConfigureTestApplicationBuilder();

            webBuilder.Configure(_configure);

            WebHostBuilderCustomization?.Invoke(webBuilder);
        });

        HostBuilderCustomization?.Invoke(hostBuilder);

        return hostBuilder;
    }
}
