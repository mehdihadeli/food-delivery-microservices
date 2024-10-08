using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using Elastic.Channels;
using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.EntityFrameworkCore.Destructurers;
using Serilog.Settings.Configuration;
using Serilog.Sinks.Grafana.Loki;
using Serilog.Sinks.Spectre;

namespace BuildingBlocks.Logging;

public static class DependencyInjectionExtensions
{
    public static WebApplicationBuilder AddCustomSerilog(
        this WebApplicationBuilder builder,
        Action<LoggerConfiguration>? extraConfigure = null,
        Action<SerilogOptions>? configurator = null
    )
    {
        var serilogOptions = builder.Configuration.BindOptions<SerilogOptions>();
        configurator?.Invoke(serilogOptions);

        // add option to the dependency injection
        builder.Services.AddValidationOptions<SerilogOptions>(opt => configurator?.Invoke(opt));

        // https://andrewlock.net/creating-a-rolling-file-logging-provider-for-asp-net-core-2-0/
        // https://github.com/serilog/serilog-extensions-hosting
        // https://andrewlock.net/adding-serilog-to-the-asp-net-core-generic-host/
        // Serilog replace `ILoggerFactory`,It replaces microsoft `LoggerFactory` class with `SerilogLoggerFactory`, so `ConsoleLoggerProvider` and other default microsoft logger providers don't instantiate at all with serilog
        builder.Host.UseSerilog(
            (context, serviceProvider, loggerConfiguration) =>
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
                    .Enrich.WithExceptionDetails(
                        new DestructuringOptionsBuilder()
                            .WithDefaultDestructurers()
                            .WithDestructurers(new[] { new DbUpdateExceptionDestructurer() })
                    );

                // https://github.com/serilog/serilog-settings-configuration
                loggerConfiguration.ReadFrom.Configuration(
                    context.Configuration,
                    new ConfigurationReaderOptions { SectionName = nameof(SerilogOptions), }
                );

                if (serilogOptions.UseConsole)
                {
                    // https://github.com/serilog/serilog-sinks-async
                    loggerConfiguration.WriteTo.Async(writeTo =>
                        // https://github.com/lucadecamillis/serilog-sinks-spectre
                        writeTo.Spectre(outputTemplate: serilogOptions.LogTemplate)
                    );
                }

                // https://github.com/serilog/serilog-sinks-async
                if (!string.IsNullOrEmpty(serilogOptions.ElasticSearchUrl))
                {
                    // elasticsearch sink internally is async
                    // https://www.nuget.org/packages/Elastic.Serilog.Sinks
                    loggerConfiguration.WriteTo.Elasticsearch(
                        new[] { new Uri(serilogOptions.ElasticSearchUrl) },
                        opts =>
                        {
                            opts.DataStream = new DataStreamName(
                                $"{builder.Environment.ApplicationName}-{builder.Environment.EnvironmentName}-{DateTime.Now:yyyy-MM}"
                            );
                            opts.BootstrapMethod = BootstrapMethod.Failure;
                            opts.ConfigureChannel = channelOpts =>
                            {
                                channelOpts.BufferOptions = new BufferOptions { ExportMaxConcurrency = 10 };
                            };
                        }
                    );
                }

                // https://github.com/serilog-contrib/serilog-sinks-grafana-loki
                if (!string.IsNullOrEmpty(serilogOptions.GrafanaLokiUrl))
                {
                    loggerConfiguration.WriteTo.GrafanaLoki(
                        serilogOptions.GrafanaLokiUrl,
                        new[]
                        {
                            new LokiLabel { Key = "service", Value = "food-delivery" }
                        },
                        ["app",]
                    );
                }

                if (!string.IsNullOrEmpty(serilogOptions.SeqUrl))
                {
                    // seq sink internally is async
                    loggerConfiguration.WriteTo.Seq(serilogOptions.SeqUrl);
                }

                // https://github.com/serilog/serilog-sinks-opentelemetry
                if (serilogOptions.ExportLogsToOpenTelemetry)
                {
                    // export logs from serilog to opentelemetry
                    loggerConfiguration.WriteTo.OpenTelemetry();
                }

                if (!string.IsNullOrEmpty(serilogOptions.LogPath))
                {
                    loggerConfiguration.WriteTo.Async(writeTo =>
                        writeTo.File(
                            serilogOptions.LogPath,
                            outputTemplate: serilogOptions.LogTemplate,
                            rollingInterval: RollingInterval.Day,
                            rollOnFileSizeLimit: true
                        )
                    );
                }
            }
        );

        return builder;
    }
}
