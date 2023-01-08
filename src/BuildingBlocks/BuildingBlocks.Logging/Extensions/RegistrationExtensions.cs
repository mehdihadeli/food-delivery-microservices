using BuildingBlocks.Core.Extensions;
using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Exceptions;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.Elasticsearch;

namespace BuildingBlocks.Logging;

public static class RegistrationExtensions
{
    public static WebApplicationBuilder AddCustomSerilog(
        this WebApplicationBuilder builder,
        string sectionName = "Serilog",
        Action<LoggerConfiguration>? extraConfigure = null)
    {
        var serilogOptions = builder.Configuration.BindOptions<SerilogOptions>(sectionName);

        // https://andrewlock.net/creating-a-rolling-file-logging-provider-for-asp-net-core-2-0/
        // https://github.com/serilog/serilog-extensions-hosting
        // https://andrewlock.net/adding-serilog-to-the-asp-net-core-generic-host/
        // Serilog replace `ILoggerFactory`,It replaces microsoft `LoggerFactory` class with `SerilogLoggerFactory`, so `ConsoleLoggerProvider` and other default microsoft logger providers don't instantiate at all with serilog
        builder.Host.UseSerilog((context, serviceProvider, loggerConfiguration) =>
        {
            extraConfigure?.Invoke(loggerConfiguration);

            loggerConfiguration
                .Enrich.WithProperty("Application", builder.Environment.ApplicationName)

                // .Enrich.WithSpan()
                // .Enrich.WithBaggage()
                .Enrich.WithCorrelationIdHeader()
                .Enrich.FromLogContext()

                // https://github.com/serilog/serilog-enrichers-environment
                .Enrich.WithEnvironmentName()
                .Enrich.WithMachineName()

                // https://rehansaeed.com/logging-with-serilog-exceptions/
                .Enrich.WithExceptionDetails();

            // https://github.com/serilog/serilog-settings-configuration
            loggerConfiguration.ReadFrom.Configuration(context.Configuration, sectionName: sectionName);

            if (serilogOptions.UseConsole)
            {
                if (serilogOptions.UseElasticsearchJsonFormatter)
                {
                    // https://github.com/serilog/serilog-sinks-async
                    // https://github.com/serilog-contrib/serilog-sinks-elasticsearch#elasticsearch-formatters
                    loggerConfiguration.WriteTo.Async(writeTo =>
                        writeTo.Console(new ExceptionAsObjectJsonFormatter(renderMessage: true)));
                }
                else
                {
                    // https://github.com/serilog/serilog-sinks-async
                    loggerConfiguration.WriteTo.Async(
                        writeTo => writeTo.Console(outputTemplate: serilogOptions.LogTemplate));
                }
            }

            // https://github.com/serilog/serilog-sinks-async
            if (!string.IsNullOrEmpty(serilogOptions.ElasticSearchUrl))
            {
                // elasticsearch sink internally is async
                // https://github.com/serilog-contrib/serilog-sinks-elasticsearch
                loggerConfiguration.WriteTo.Elasticsearch(new(new Uri(serilogOptions.ElasticSearchUrl))
                {
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
                    CustomFormatter = new ExceptionAsObjectJsonFormatter(renderMessage: true),
                    IndexFormat =
                        $"{builder.Environment.ApplicationName}-{builder.Environment.EnvironmentName}-{DateTime.Now:yyyy-MM}"
                });
            }

            if (!string.IsNullOrEmpty(serilogOptions.SeqUrl))
            {
                // seq sink internally is async
                loggerConfiguration.WriteTo.Seq(serilogOptions.SeqUrl);
            }

            if (!string.IsNullOrEmpty(serilogOptions.LogPath))
            {
                loggerConfiguration.WriteTo.Async(writeTo =>
                    writeTo.File(
                        serilogOptions.LogPath,
                        outputTemplate: serilogOptions.LogTemplate,
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true));
            }
        });

        return builder;
    }
}
