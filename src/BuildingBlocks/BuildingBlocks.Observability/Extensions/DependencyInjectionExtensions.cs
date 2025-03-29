using System.Diagnostics;
using System.Reflection;
using BuildingBlocks.Core.Diagnostics;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollectionExtensions;
using BuildingBlocks.Core.Persistence;
using Grafana.OpenTelemetry;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shared.Observability;

namespace BuildingBlocks.Observability.Extensions;

// https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-otlp-example
// https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-prgrja-example
// https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-prgrja-example
// https://blog.codingmilitia.com/2023/09/05/observing-dotnet-microservices-with-opentelemetry-logs-traces-metrics/
public static class DependencyInjectionExtensions
{
    public static WebApplicationBuilder AddCustomObservability(
        this WebApplicationBuilder builder,
        Action<ObservabilityOptionsConfigurator>? configureOptions = null
    )
    {
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;

        builder.Services.AddConfigurationOptions<ObservabilityOptions>(nameof(ObservabilityOptions));
        var observabilityOptions = builder.Configuration.BindOptions<ObservabilityOptions>();

        var optionsConfigurations = new ObservabilityOptionsConfigurator();
        configureOptions?.Invoke(optionsConfigurations);

        if (observabilityOptions is { MetricsEnabled: false, TracingEnabled: false, LoggingEnabled: false })
        {
            return builder;
        }

        void ConfigureResourceBuilder(ResourceBuilder resourceBuilder)
        {
            resourceBuilder.AddAttributes([new("service.environment", builder.Environment.EnvironmentName)]);

            resourceBuilder.AddService(
                serviceName: observabilityOptions.ServiceName ?? builder.Environment.ApplicationName,
                serviceVersion: Assembly.GetCallingAssembly().GetName().Version?.ToString() ?? "unknown",
                serviceInstanceId: Environment.MachineName
            );
        }

        if (observabilityOptions.LoggingEnabled)
        {
            // logging
            // opentelemtry logging works with .net default logging providers and doesn't work for `serilog`, in serilog we should enable `WriteToProviders=true`
            builder.Logging.AddOpenTelemetry(options =>
            {
                var resourceBuilder = ResourceBuilder.CreateDefault();
                ConfigureResourceBuilder(resourceBuilder);
                options.SetResourceBuilder(resourceBuilder);

                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
                // this allows the state value passed to the logger.Log method to be parsed, in case it isn't a collection of KeyValuePair<string, object?>, which is the case when we use things like logger.LogInformation.
                options.ParseStateValues = true;
                // which means the message wouldn't have the placeholders replaced
                options.IncludeFormattedMessage = true;

                // add some metadata to exported logs
                options.SetResourceBuilder(
                    ResourceBuilder
                        .CreateDefault()
                        .AddService(
                            observabilityOptions.ServiceName ?? builder.Environment.ApplicationName,
                            serviceVersion: Assembly.GetCallingAssembly().GetName().Version?.ToString() ?? "unknown",
                            serviceInstanceId: Environment.MachineName
                        )
                );

                options.AddLoggingExporters(observabilityOptions);
            });
        }

        if (observabilityOptions is { MetricsEnabled: false, TracingEnabled: false })
        {
            return builder;
        }

        OpenTelemetryBuilder otel = null!;

        if (observabilityOptions.MetricsEnabled || observabilityOptions.TracingEnabled)
        {
            // metrics and tracing
            otel = builder.Services.AddOpenTelemetry();
            otel.ConfigureResource(ConfigureResourceBuilder);
        }

        if (observabilityOptions.MetricsEnabled)
        {
            otel.WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddProcessInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter(MassTransit.Monitoring.InstrumentationOptions.MeterName)
                    // metrics provides by ASP.NET Core in .NET 8
                    .AddView(
                        "http.server.request.duration",
                        new ExplicitBucketHistogramConfiguration
                        {
                            Boundaries = [0, 0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10],
                        }
                    )
                    .AddMeter("System.Runtime")
                    .AddMeter("Microsoft.AspNetCore.Hosting")
                    .AddMeter("Microsoft.AspNetCore.Server.Kestrel");

                if (!string.IsNullOrEmpty(DiagnosticsConstant.ApplicationInstrumentationName))
                {
                    metrics.AddMeter(DiagnosticsConstant.ApplicationInstrumentationName);
                }

                AddMetricsExporter(observabilityOptions, metrics);

                optionsConfigurations.ConfigureMeterProvider?.Invoke(metrics);
            });
        }

        if (observabilityOptions.TracingEnabled)
        {
            otel.WithTracing(tracing =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    // We want to view all traces in development
                    tracing.SetSampler(new AlwaysOnSampler());
                }

                tracing
                    .SetErrorStatusOnException()
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddSource(MigrationWorker.ActivitySource.Name)
                    .AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation(instrumentationOptions =>
                    {
                        instrumentationOptions.RecordException = true;
                    })
                    .AddEntityFrameworkCoreInstrumentation(instrumentationOptions =>
                    {
                        instrumentationOptions.SetDbStatementForText = true;
                    })
                    .AddSource(MassTransit.Logging.DiagnosticHeaders.DefaultListenerName)
                    .AddNpgsql()
                    // `AddSource` for adding custom activity sources

                    // metrics provides by ASP.NET Core in .NET 8
                    .AddSource("Microsoft.AspNetCore.Hosting")
                    .AddSource("Microsoft.AspNetCore.Server.Kestrel");

                if (!string.IsNullOrEmpty(DiagnosticsConstant.ApplicationInstrumentationName))
                {
                    tracing.AddSource(DiagnosticsConstant.ApplicationInstrumentationName);
                }

                AddTracingExporter(observabilityOptions, tracing);

                optionsConfigurations.ConfigureTracerProvider?.Invoke(tracing);
            });
        }

        return builder;
    }

    private static void AddTracingExporter(ObservabilityOptions observabilityOptions, TracerProviderBuilder tracing)
    {
        if (observabilityOptions.UseJaegerExporter)
        {
            ArgumentNullException.ThrowIfNull(observabilityOptions.JaegerOptions);
            // https://github.com/open-telemetry/opentelemetry-dotnet/tree/e330e57b04fa3e51fe5d63b52bfff891fb5b7961/docs/trace/getting-started-jaeger
            // `OpenTelemetry.Exporter.Jaeger` package and `AddJaegerExporter` to use Http endpoint (http://localhost:14268/api/traces) is deprecated, and we should use `OpenTelemetry.Exporter.OpenTelemetryProtocol` and `AddOtlpExporter` with OTLP port `4317` on Jaeger
            // tracing.AddJaegerExporter(
            //     x => x.Endpoint = new Uri(observabilityOptions.JaegerOptions.HttpExporterEndpoint)); // http://localhost:14268/api/traces
            tracing.AddOtlpExporter(x =>
            {
                x.Endpoint = new Uri(observabilityOptions.JaegerOptions.OTLPGrpcExporterEndpoint);
                x.Protocol = OtlpExportProtocol.Grpc;
            });
        }

        if (observabilityOptions.UseZipkinExporter)
        {
            ArgumentNullException.ThrowIfNull(observabilityOptions.ZipkinOptions);
            // https://github.com/open-telemetry/opentelemetry-dotnet/tree/e330e57b04fa3e51fe5d63b52bfff891fb5b7961/src/OpenTelemetry.Exporter.Zipkin
            tracing.AddZipkinExporter(x =>
                x.Endpoint = new Uri(observabilityOptions.ZipkinOptions.HttpExporterEndpoint)
            ); // "http://localhost:9411/api/v2/spans"
        }

        if (observabilityOptions.UseConsoleExporter)
        {
            tracing.AddConsoleExporter();
        }

        if (observabilityOptions.UseOTLPGrpcExporter)
        {
            ArgumentNullException.ThrowIfNull(observabilityOptions.OTLPOptions);
            tracing.AddOtlpExporter(x =>
            {
                x.Endpoint = new Uri(observabilityOptions.OTLPOptions.OTLPGrpcExporterEndpoint);
                x.Protocol = OtlpExportProtocol.Grpc;
            });
        }

        if (observabilityOptions.UseOTLPHttpExporter)
        {
            ArgumentNullException.ThrowIfNull(observabilityOptions.OTLPOptions);
            tracing.AddOtlpExporter(x =>
            {
                x.Endpoint = new Uri(observabilityOptions.OTLPOptions.OTLPHttpExporterEndpoint);
                x.Protocol = OtlpExportProtocol.HttpProtobuf;
            });
        }

        if (observabilityOptions.UseAspireOTLPExporter)
        {
            // we can just one `AddOtlpExporter` and in development use `aspire-dashboard` OTLP endpoint address as `OTLPExporterEndpoint` and in production we can use `otel-collector` OTLP endpoint address
            tracing.AddOtlpExporter(x =>
            {
                x.Endpoint = new Uri(observabilityOptions.AspireDashboardOTLPOptions.OTLPGrpcExporterEndpoint);
                x.Protocol = OtlpExportProtocol.Grpc;
            });
        }

        if (observabilityOptions.UseGrafanaExporter)
        {
            // https://github.com/grafana/grafana-opentelemetry-dotnet/blob/main/docs/configuration.md#aspnet-core
            // https://github.com/grafana/grafana-opentelemetry-dotnet/
            // https://github.com/grafana/grafana-opentelemetry-dotnet/blob/main/docs/configuration.md#sending-to-an-agent-or-collector-via-otlp
            // https://grafana.com/docs/grafana-cloud/monitor-applications/application-observability/instrument/dotnet/
            tracing.UseGrafana();
        }
    }

    private static void AddMetricsExporter(ObservabilityOptions observabilityOptions, MeterProviderBuilder metrics)
    {
        if (observabilityOptions.UsePrometheusExporter)
        {
            // https://github.com/open-telemetry/opentelemetry-dotnet/tree/e330e57b04fa3e51fe5d63b52bfff891fb5b7961/src/OpenTelemetry.Exporter.Prometheus.AspNetCore
            // https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Exporter.Prometheus.AspNetCore/README.md
            // for exporting app metrics to `/metrics` endpoint
            // http://localhost:5000/metrics
            metrics.AddPrometheusExporter(o => o.DisableTotalNameSuffixForCounters = true);
        }

        if (observabilityOptions.UseConsoleExporter)
        {
            metrics.AddConsoleExporter();
        }

        if (observabilityOptions.UseOTLPGrpcExporter)
        {
            ArgumentNullException.ThrowIfNull(observabilityOptions.OTLPOptions);
            metrics.AddOtlpExporter(x =>
            {
                x.Endpoint = new Uri(observabilityOptions.OTLPOptions.OTLPGrpcExporterEndpoint);
                x.Protocol = OtlpExportProtocol.Grpc;
            });
        }

        if (observabilityOptions.UseOTLPHttpExporter)
        {
            ArgumentNullException.ThrowIfNull(observabilityOptions.OTLPOptions);
            metrics.AddOtlpExporter(x =>
            {
                x.Endpoint = new Uri(observabilityOptions.OTLPOptions.OTLPHttpExporterEndpoint);
                x.Protocol = OtlpExportProtocol.HttpProtobuf;
            });
        }

        if (observabilityOptions.UseAspireOTLPExporter)
        {
            // we can just one `AddOtlpExporter` and in development use `aspire-dashboard` OTLP endpoint address as `OTLPExporterEndpoint` and in production we can use `otel-collector` OTLP endpoint address
            metrics.AddOtlpExporter(x =>
            {
                x.Endpoint = new Uri(observabilityOptions.AspireDashboardOTLPOptions.OTLPGrpcExporterEndpoint);

                x.Protocol = OtlpExportProtocol.Grpc;
            });
        }

        if (observabilityOptions.UseGrafanaExporter)
        {
            // https://github.com/grafana/grafana-opentelemetry-dotnet/blob/main/docs/configuration.md#aspnet-core
            // https://github.com/grafana/grafana-opentelemetry-dotnet/
            // https://github.com/grafana/grafana-opentelemetry-dotnet/blob/main/docs/configuration.md#sending-to-an-agent-or-collector-via-otlp
            // https://grafana.com/docs/grafana-cloud/monitor-applications/application-observability/instrument/dotnet/
            metrics.UseGrafana();
        }
    }

    private static void AddLoggingExporters(
        this OpenTelemetryLoggerOptions openTelemetryLoggerOptions,
        ObservabilityOptions observabilityOptions
    )
    {
        if (observabilityOptions.UseOTLPGrpcExporter)
        {
            ArgumentNullException.ThrowIfNull(observabilityOptions.OTLPOptions);
            openTelemetryLoggerOptions.AddOtlpExporter(x =>
            {
                x.Endpoint = new Uri(observabilityOptions.OTLPOptions.OTLPGrpcExporterEndpoint);
                x.Protocol = OtlpExportProtocol.Grpc;
            });
        }

        if (observabilityOptions.UseOTLPHttpExporter)
        {
            ArgumentNullException.ThrowIfNull(observabilityOptions.OTLPOptions);
            openTelemetryLoggerOptions.AddOtlpExporter(x =>
            {
                x.Endpoint = new Uri(observabilityOptions.OTLPOptions.OTLPHttpExporterEndpoint);
                x.Protocol = OtlpExportProtocol.HttpProtobuf;
            });
        }

        if (observabilityOptions.UseAspireOTLPExporter)
        {
            // we can just one `AddOtlpExporter` and in development use `aspire-dashboard` OTLP endpoint address as `OTLPExporterEndpoint` and in production we can use `otel-collector` OTLP endpoint address
            openTelemetryLoggerOptions.AddOtlpExporter(x =>
            {
                x.Endpoint = new Uri(observabilityOptions.AspireDashboardOTLPOptions.OTLPGrpcExporterEndpoint);

                x.Protocol = OtlpExportProtocol.Grpc;
            });
        }

        if (observabilityOptions.UseGrafanaExporter)
        {
            // https://github.com/grafana/grafana-opentelemetry-dotnet/
            // https://github.com/grafana/grafana-opentelemetry-dotnet/blob/main/docs/configuration.md#aspnet-core
            // https://grafana.com/docs/grafana-cloud/monitor-applications/application-observability/instrument/dotnet/
            openTelemetryLoggerOptions.UseGrafana();
        }

        if (observabilityOptions.UseConsoleExporter)
        {
            openTelemetryLoggerOptions.AddConsoleExporter();
        }
    }
}
