using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Tests.Shared.Extensions;
using Xunit;
using Environments = BuildingBlocks.Core.Web.Environments;

namespace Tests.Shared.Factory;

/// <summary>
/// This WebApplicationFactory only use for testing web components without needing Program entrypoint and it doesn't work for web app with Program file and for this case we should use original WebApplicationFactory.
/// </summary>
/// <typeparam name="TEntryPoint"></typeparam>
class WebApplicationFactoryWithHost<TEntryPoint>(
    Action<IServiceCollection> configureServices,
    Action<IApplicationBuilder> configure,
    string[]? args = null
) : WebApplicationFactory<TEntryPoint>
    where TEntryPoint : class
{
    readonly string[] _args = args ?? [];

    public ITestOutputHelper? TestOutputHelper { get; set; }
    public Action<IHostBuilder>? HostBuilderCustomization { get; set; }
    public Action<IWebHostBuilder>? WebHostBuilderCustomization { get; set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
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
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Test);
        builder.UseContentRoot(".");

        // // UseSerilog on WebHostBuilder is absolute so we should use IHostBuilder
        // builder.UseSerilog(
        //     (ctx, loggerConfiguration) =>
        //     {
        //         //https://github.com/trbenning/serilog-sinks-xunit
        //         if (TestOutputHelper is not null)
        //         {
        //             loggerConfiguration.WriteTo.TestOutput(
        //                 TestOutputHelper,
        //                 LogEventLevel.Information,
        //                 "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level} - {Message:lj}{NewLine}{Exception}"
        //             );
        //         }
        //     }
        // );

        // create startup with these configs
        builder.ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.ConfigureServices(configureServices);

            //https://github.com/dotnet/aspnetcore/issues/37680#issuecomment-1331559463
            //https://github.com/dotnet/aspnetcore/issues/45319#issuecomment-1334355103
            // Set this so that the async context flows
            configure.ConfigureTestApplicationBuilder();

            webBuilder.Configure(configure);

            WebHostBuilderCustomization?.Invoke(webBuilder);
        });

        HostBuilderCustomization?.Invoke(builder);

        return base.CreateHost(builder);
    }
}
