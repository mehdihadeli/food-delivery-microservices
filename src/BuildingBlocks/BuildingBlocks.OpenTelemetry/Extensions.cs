using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace BuildingBlocks.OpenTelemetry;

// Ref: https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-with-otel
public static class Extensions
{
    public static WebApplicationBuilder AddCustomOpenTelemetry(
        this WebApplicationBuilder builder,
        Action<OpenTelemetryOptions>? configurator = null
    )
    {
        var options = builder.Configuration.BindOptions<OpenTelemetryOptions>();
        configurator?.Invoke(options);

        // add option to the dependency injection
        builder.Services.AddValidationOptions<OpenTelemetryOptions>(opt => configurator?.Invoke(opt));

        var resourceBuilder = ResourceBuilder.CreateDefault().AddService(builder.Environment.ApplicationName);

        builder
            .Services.AddOpenTelemetry()
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .SetResourceBuilder(resourceBuilder)
                    // https://github.com/open-telemetry/opentelemetry-dotnet-contrib/issues/326
                    .AddSource("MassTransit")
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation();

                SetTracingExporters(options, tracerProviderBuilder);
            });

        builder.Logging.AddOpenTelemetry(o =>
        {
            o.SetResourceBuilder(resourceBuilder);

            SetLogExporters(options, o);
        });

        builder
            .Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(resourceBuilder)
                    .AddPrometheusExporter()
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEventCountersInstrumentation(c =>
                    {
                        // https://learn.microsoft.com/en-us/dotnet/core/diagnostics/available-counters
                        c.AddEventSources(
                            "Microsoft.AspNetCore.Hosting",
                            "System.Net.Http",
                            "System.Net.Sockets",
                            "System.Net.NameResolution",
                            "System.Net.Security"
                        );
                    });

                SetMetricsExporters(options, metrics);
            });

        builder.Services.Configure<OpenTelemetryLoggerOptions>(opt =>
        {
            opt.IncludeScopes = true;
            opt.ParseStateValues = true;
            opt.IncludeFormattedMessage = true;
        });

        return builder;
    }

    private static void SetLogExporters(OpenTelemetryOptions options, OpenTelemetryLoggerOptions loggerOptions)
    {
        switch (options.LogExporterType)
        {
            case nameof(LogExporterType.Console):
                loggerOptions.AddConsoleExporter();
                break;

            case nameof(LogExporterType.OTLP):
                loggerOptions.AddOtlpExporter(
                    (otelExporterOptions, logRecorderOptions) =>
                    {
                        otelExporterOptions.Endpoint = new Uri(options.OtlpOptions.OTLPEndpoint);
                    }
                );
                break;
            case nameof(LogExporterType.None):
                break;
        }
    }

    private static void SetMetricsExporters(OpenTelemetryOptions options, MeterProviderBuilder metrics)
    {
        switch (options.MetricsExporterType)
        {
            case nameof(MetricsExporterType.Console):
                metrics.AddConsoleExporter(
                    (exporterOptions, metricReaderOptions) =>
                    {
                        exporterOptions.Targets = ConsoleExporterOutputTargets.Console;
                        metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 10000;
                    }
                );
                break;

            case nameof(MetricsExporterType.OTLP):
                metrics.AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri(options.OtlpOptions.OTLPEndpoint);
                });
                break;
            case nameof(MetricsExporterType.None):
                break;
        }
    }

    private static void SetTracingExporters(OpenTelemetryOptions options, TracerProviderBuilder tracerProviderBuilder)
    {
        switch (options.TracingExporterType)
        {
            case nameof(TracingExporterType.Console):
                tracerProviderBuilder.AddConsoleExporter();
                break;

            case nameof(TracingExporterType.OTLP):
                tracerProviderBuilder.AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri(options.OtlpOptions.OTLPEndpoint);
                });
                break;
            case nameof(TracingExporterType.Zipkin):
                tracerProviderBuilder.AddZipkinExporter(x =>
                {
                    x.Endpoint = new Uri(options.ZipkinOptions.Endpoint);
                });
                break;
            case nameof(TracingExporterType.Jaeger):
                tracerProviderBuilder.AddJaegerExporter(x =>
                {
                    x.AgentHost = options.JaegerOptions.AgentHost;
                    x.AgentPort = options.JaegerOptions.AgentPort;
                    x.MaxPayloadSizeInBytes = 4096;
                    x.ExportProcessorType = ExportProcessorType.Batch;
                    x.BatchExportProcessorOptions = new BatchExportProcessorOptions<System.Diagnostics.Activity>
                    {
                        MaxQueueSize = 2048,
                        ScheduledDelayMilliseconds = 5000,
                        ExporterTimeoutMilliseconds = 30000,
                        MaxExportBatchSize = 512,
                    };
                });
                break;
            case nameof(TracingExporterType.None):
                break;
        }
    }
}
