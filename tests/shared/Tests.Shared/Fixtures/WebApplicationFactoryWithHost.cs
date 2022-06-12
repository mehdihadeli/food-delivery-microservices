using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Tests.Shared.Fixtures;

// Ref: https://milestone.topics.it/2021/04/28/you-wanna-test-http.html
public class WebApplicationFactoryWithHost<TEntryPoint> :
    WebApplicationFactory<TEntryPoint>
    where TEntryPoint : class
{
    private readonly Action<IServiceCollection> _configureServices;
    private readonly Action<IApplicationBuilder> _configure;
    private readonly string[] _args;

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

    protected override IHostBuilder CreateHostBuilder()
    {
        var hostBuilder = Host.CreateDefaultBuilder(_args);
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
