using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollectionExtensions;
using Elastic.Channels;
using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Settings.Configuration;
using Serilog.Sinks.Grafana.Loki;
using Serilog.Sinks.Spectre;

namespace BuildingBlocks.SerilogLogging.Extensions;

public static class DependencyInjectionExtensions
{
    public static IHostApplicationBuilder AddCustomSerilog(
        this IHostApplicationBuilder builder,
        Action<LoggerConfiguration>? extraConfigure = null,
        Action<SerilogOptions>? configurator = null
    )
    {
        var serilogOptions = builder.Configuration.BindOptions<SerilogOptions>();
        configurator?.Invoke(serilogOptions);

        // add option to the dependency injection
        builder.Services.AddConfigurationOptions<SerilogOptions>(configurator: opt => configurator?.Invoke(opt));

        // https://github.com/serilog/serilog-extensions-hosting
        // https://github.com/serilog/serilog-aspnetcore#two-stage-initialization
        // https://stackoverflow.com/a/78467358
        // - Routes framework log messages through Serilog - get other sinks from top level definition
        // - For preventing duplicate write logs by .net default logs provider, we should remove them for serilog because we enabled `writeToProviders=true` to send logs to .net default logs provider using by opentelemetry.
        builder.Logging.ClearProviders();
        builder.Services.AddSerilog(
            (sp, loggerConfiguration) =>
            {
                // The downside of initializing Serilog in top level is that services from the ASP.NET Core host, including the appsettings.json configuration and dependency injection, aren't available yet.
                // setup sinks that related to `configuration` here instead of top level serilog configuration
                // https://github.com/serilog/serilog-settings-configuration
                // This also applies MinimumLevel + Overrides
                loggerConfiguration.ReadFrom.Configuration(
                    builder.Configuration,
                    new ConfigurationReaderOptions { SectionName = nameof(SerilogOptions) }
                );

                extraConfigure?.Invoke(loggerConfiguration);

                loggerConfiguration
                    .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
                    .Enrich.FromLogContext()
                    .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder().WithDefaultDestructurers());

                if (serilogOptions.UseConsole)
                {
                    // https://github.com/serilog/serilog-sinks-async
                    // https://github.com/lucadecamillis/serilog-sinks-spectre
                    loggerConfiguration.WriteTo.Async(writeTo =>
                        writeTo.Spectre(outputTemplate: serilogOptions.LogTemplate)
                    );
                }

                // https://github.com/serilog/serilog-sinks-async
                if (!string.IsNullOrEmpty(serilogOptions.ElasticSearchUrl))
                {
                    // elasticsearch sink internally is async
                    // https://www.nuget.org/packages/Elastic.Serilog.Sinks
                    loggerConfiguration.WriteTo.Elasticsearch(
                        [new Uri(serilogOptions.ElasticSearchUrl)],
                        opts =>
                        {
                            opts.DataStream = new DataStreamName(
                                $"{
                                    builder.Environment.ApplicationName
                                }-{
                                    builder.Environment.EnvironmentName
                                }-{
                                    DateTime.Now:yyyy-MM}"
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
                            new LokiLabel { Key = "service", Value = "vertical-slice-template" },
                        },
                        ["app"]
                    );
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
                            rollOnFileSizeLimit: true
                        )
                    );
                }
            },
            true,
            // https://stackoverflow.com/a/78467358/581476
            // - I don't want to use `serilog-sinks-opentelemetry` package because I want to use OpenTelemetry `builder.SeilogLogging.AddOpenTelemetry` and its capabilities like config multiple exporters by code.
            // - For sending logs to OpenTelemetry we use `builder.SeilogLogging.AddOpenTelemetry` and it works with .net builtin logging providers (ILoggerProviders) but serilog by default doesn't send logs to
            //   these providers and as a result serilog can't send telemetry logs, to enable it we should use `writeToProviders` in Serilog.
            // - By default, `Serilog` does not write events to `ILoggerProviders` registered through the `Microsoft.Extensions.SeilogLogging` API. Normally, equivalent Serilog sinks are used in place of providers. Specify true to write events to all providers.
            writeToProviders: serilogOptions.ExportLogsToOpenTelemetry
        );

        return builder;
    }
}
