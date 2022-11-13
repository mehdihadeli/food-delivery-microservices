using Ardalis.GuardClauses;
using BuildingBlocks.Core.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace BuildingBlocks.OpenTelemetry;

public static class Extensions
{
    public static WebApplicationBuilder AddOTelLogs(this WebApplicationBuilder builder)
    {
        var resourceBuilder = ResourceBuilder.CreateDefault().AddService(builder.Environment.ApplicationName);
        var options = builder.Configuration.GetOptions<OpenTelemetryOptions>(nameof(OpenTelemetryOptions));
        Guard.Against.Null(options);

        builder.Logging.AddOpenTelemetry(o =>
        {
            o.SetResourceBuilder(resourceBuilder);

            switch (options.LogExporterType)
            {
                case nameof(ExporterType.Console):
                    o.AddConsoleExporter();
                    break;

                case nameof(ExporterType.OTLP):
                    o.AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(options.OTLPEndpoint);
                    });
                    break;
                case nameof(ExporterType.None):
                    break;
            }
        });

        builder.Services.Configure<OpenTelemetryLoggerOptions>(opt =>
        {
            opt.IncludeScopes = true;
            opt.ParseStateValues = true;
            opt.IncludeFormattedMessage = true;
        });

        return builder;
    }

    public static WebApplicationBuilder AddOTelTracing(this WebApplicationBuilder builder)
    {
        var resourceBuilder = ResourceBuilder.CreateDefault().AddService(builder.Environment.ApplicationName);

        var options = builder.Configuration.GetOptions<OpenTelemetryOptions>(nameof(OpenTelemetryOptions));
        Guard.Against.Null(options);

        builder.Services.AddOpenTelemetryTracing(
            tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .SetResourceBuilder(resourceBuilder)
                    .AddSource("MassTransit") // https://github.com/open-telemetry/opentelemetry-dotnet-contrib/issues/326
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation();

                switch (options.TracingExporterType)
                {
                    case nameof(ExporterType.Console):
                        tracerProviderBuilder.AddConsoleExporter();
                        break;

                    case nameof(ExporterType.OTLP):
                        tracerProviderBuilder.AddOtlpExporter(otlpOptions =>
                        {
                            otlpOptions.Endpoint = new Uri(options.OTLPEndpoint);
                        });
                        break;
                    case nameof(ExporterType.Jaeger):
                        break;
                    case nameof(ExporterType.None):
                        break;
                }
            }
        );

        // For options which can be configured from code only.
        builder.Services.Configure<AspNetCoreInstrumentationOptions>(
            aspNetCoreInstrumentationOptions =>
            {
                aspNetCoreInstrumentationOptions.Filter = _ => true;
            });

        return builder;
    }

    public static WebApplicationBuilder AddOTelMetrics(this WebApplicationBuilder builder)
    {
        var resourceBuilder = ResourceBuilder.CreateDefault().AddService(builder.Environment.ApplicationName);
        var options = builder.Configuration.GetOptions<OpenTelemetryOptions>(nameof(OpenTelemetryOptions));
        Guard.Against.Null(options);

        builder.Services.AddOpenTelemetryMetrics(metrics =>
        {
            metrics.SetResourceBuilder(resourceBuilder)
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
                        "System.Net.Security");
                });

            switch (options.TracingExporterType)
            {
                case nameof(ExporterType.Console):
                    metrics.AddConsoleExporter((exporterOptions, metricReaderOptions) =>
                    {
                        exporterOptions.Targets = ConsoleExporterOutputTargets.Console;
                        metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 10000;
                    });
                    break;

                case nameof(ExporterType.OTLP):
                    metrics.AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(options.OTLPEndpoint);
                    });
                    break;
                case nameof(ExporterType.Jaeger):
                    break;
                case nameof(ExporterType.None):
                    break;
            }
        });

        return builder;
    }
}
