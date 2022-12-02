using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Tests.Shared.Factory;

// Ref: https://milestone.topics.it/2021/04/28/you-wanna-test-http.html
// https://github.com/mehdihadeli/WebApplicationFactoryWithHost-Sample
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

    public WebApplicationFactoryWithHost(Action<IServiceCollection> configureServices,
        Action<IApplicationBuilder> configure, string[]? args = null)
    {
        _configureServices = configureServices;
        _configure = configure;
        _args = args ?? Array.Empty<string>();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // change existing services ...
            if (TestOutputHelper != null)
                services.AddLogging(b => b.AddXUnit(TestOutputHelper));
        });

        return base.CreateHost(builder);
    }

    // This creates a new host, even if there is no program file or startup (EntryPoint) for finding the CreateDefaultBuilder
    protected override IHostBuilder CreateHostBuilder()
    {
        var hostBuilder = Host.CreateDefaultBuilder(_args);
        // create startup with these configs
        hostBuilder.ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.ConfigureServices(_configureServices);
            webBuilder.Configure(_configure);

            WebHostBuilderCustomization?.Invoke(webBuilder);
        });

        HostBuilderCustomization?.Invoke(hostBuilder);

        return hostBuilder;
    }
}
