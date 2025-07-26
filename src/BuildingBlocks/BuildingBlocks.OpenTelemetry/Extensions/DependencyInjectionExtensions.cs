using System.Diagnostics;
using System.Reflection;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using BuildingBlocks.Core.Diagnostics;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Persistence;
using Grafana.OpenTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace BuildingBlocks.OpenTelemetry.Extensions;

// https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-otlp-example
// https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-prgrja-example
// https://blog.codingmilitia.com/2023/09/05/observing-dotnet-microservices-with-opentelemetry-logs-traces-metrics/
// https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/service-defaults
public static class DependencyInjectionExtensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    public static IHostApplicationBuilder AddDefaultOpenTelemetry(
        this IHostApplicationBuilder builder,
        Action<OpenTelemetryOptionsConfigurator>? configureOptions = null
    )
    {
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;

        var instrumentationName =
            builder.Configuration.GetValue<string>(DiagnosticsConstant.InstrumentationName)
            ?? "default-instrumentation";

        var openTelemetryOptions = builder.Configuration.BindOptions<OpenTelemetryOptions>();

        var optionsConfigurations = new OpenTelemetryOptionsConfigurator();
        configureOptions?.Invoke(optionsConfigurations);

        if (openTelemetryOptions is { MetricsEnabled: false, TracingEnabled: false, LoggingEnabled: false })
        {
            return builder;
        }

        void ConfigureResourceBuilder(ResourceBuilder resourceBuilder)
        {
            resourceBuilder.AddAttributes([new("service.environment", builder.Environment.EnvironmentName)]);

            resourceBuilder.AddService(
                serviceName: openTelemetryOptions.ServiceName ?? builder.Environment.ApplicationName,
                serviceVersion: Assembly.GetCallingAssembly().GetName().Version?.ToString() ?? "unknown",
                serviceInstanceId: Environment.MachineName
            );
        }

        if (openTelemetryOptions.LoggingEnabled)
        {
            // logging
            // https://github.com/dotnet/extensions/blob/main/src/Libraries/Microsoft.Extensions.Telemetry/README.md
            if (builder.Environment.IsDevelopment())
            {
                builder.Logging.AddTraceBasedSampler();
            }

            // Enable log enrichment.
            builder.Logging.EnableEnrichment(options =>
            {
                options.CaptureStackTraces = true;
                options.IncludeExceptionMessage = true;
                options.UseFileInfoForStackTraces = true;
            });
            builder.Services.AddServiceLogEnricher(options =>
            {
                options.ApplicationName = true;
                options.BuildVersion = true;
                options.DeploymentRing = true;
                options.EnvironmentName = true;
            });

            builder.Services.AddLogEnricher<ApplicationEnricher>();

            // Enable log redaction
            builder.Logging.EnableRedaction(options =>
            {
                options.ApplyDiscriminator = true;
            });

            builder.Services.AddRedaction();

            // builder.Logging.AddGlobalBuffer();
            // builder.Logging.AddPerIncomingRequestBuffer();

            // open-telemetry logging works with .net default logging providers and doesn't work for `serilog`, in serilog we should enable `WriteToProviders=true`
            builder.Logging.AddOpenTelemetry(loggerOptions =>
            {
                var resourceBuilder = ResourceBuilder.CreateDefault();
                ConfigureResourceBuilder(resourceBuilder);
                loggerOptions.SetResourceBuilder(resourceBuilder);

                loggerOptions.IncludeScopes = true;
                // this allows the state value passed to the logger.Log method to be parsed, in case it isn't a collection of KeyValuePair<string, object?>, which is the case when we use things like logger.LogInformation.
                loggerOptions.ParseStateValues = true;
                // which means the message wouldn't have the placeholders replaced
                loggerOptions.IncludeFormattedMessage = true;

                // add some metadata to exported logs
                loggerOptions.SetResourceBuilder(
                    ResourceBuilder
                        .CreateDefault()
                        .AddService(
                            openTelemetryOptions.ServiceName ?? builder.Environment.ApplicationName,
                            serviceVersion: Assembly.GetCallingAssembly().GetName().Version?.ToString() ?? "unknown",
                            serviceInstanceId: Environment.MachineName
                        )
                );

                if (openTelemetryOptions.UseConsoleExporter)
                {
                    loggerOptions.AddConsoleExporter();
                }
            });
        }

        if (openTelemetryOptions is { MetricsEnabled: false, TracingEnabled: false })
        {
            return builder;
        }

        OpenTelemetryBuilder otel = null!;

        if (openTelemetryOptions.MetricsEnabled || openTelemetryOptions.TracingEnabled)
        {
            // metrics and tracing
            otel = builder.Services.AddOpenTelemetry();
            otel.ConfigureResource(ConfigureResourceBuilder);
        }

        if (openTelemetryOptions.TracingEnabled)
        {
            otel.WithTracing(tracing =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    // We want to view all traces in development
                    tracing.SetSampler(new AlwaysOnSampler());
                }

                tracing
                    // add an open-telemetry listener on ActivitySource listeners
                    .AddSource(instrumentationName)
                    .AddSource(MigrationWorker.ActivitySource.Name)
                    .SetErrorStatusOnException()
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;

                        // Exclude health check requests from tracing
                        options.Filter = httpContext =>
                            !(
                                httpContext.Request.Path.StartsWithSegments(
                                    HealthEndpointPath,
                                    StringComparison.InvariantCulture
                                )
                                || httpContext.Request.Path.StartsWithSegments(
                                    AlivenessEndpointPath,
                                    StringComparison.InvariantCulture
                                )
                            );
                    })
                    .AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation(instrumentationOptions =>
                    {
                        instrumentationOptions.RecordException = true;
                    })
                    .AddProcessor(new FixHttpRouteProcessor())
                    .AddEntityFrameworkCoreInstrumentation(instrumentationOptions =>
                    {
                        instrumentationOptions.SetDbStatementForText = true;
                    })
                    .AddNpgsql();

                optionsConfigurations.ConfigureTracerProvider?.Invoke(tracing);

                var useJaeger =
                    !string.IsNullOrEmpty(openTelemetryOptions.JaegerOptions.OTLPGrpcExporterEndpoint)
                    && openTelemetryOptions.JaegerOptions.Enabled;
                var useZipkin =
                    !string.IsNullOrEmpty(openTelemetryOptions.ZipkinOptions.HttpExporterEndpoint)
                    && openTelemetryOptions.ZipkinOptions.Enabled;

                if (useJaeger)
                {
                    // https://github.com/open-telemetry/opentelemetry-dotnet/tree/e330e57b04fa3e51fe5d63b52bfff891fb5b7961/docs/trace/getting-started-jaeger
                    // `OpenTelemetry.Exporter.Jaeger` package and `AddJaegerExporter` to use Http endpoint (http://localhost:14268/api/traces) is deprecated, and we should use `OpenTelemetry.Exporter.OpenTelemetryProtocol` and `AddOtlpExporter` with OTLP port `4317` on Jaeger
                    // tracing.AddJaegerExporter(
                    //     x => x.Endpoint = new Uri(OpenTelemetryOptions.JaegerOptions.HttpExporterEndpoint)); // http://localhost:14268/api/traces
                    tracing.AddOtlpExporter(x =>
                    {
                        x.Endpoint = new Uri(openTelemetryOptions.JaegerOptions.OTLPGrpcExporterEndpoint);
                        x.Protocol = OtlpExportProtocol.Grpc;
                    });
                }

                if (useZipkin)
                {
                    // https://github.com/open-telemetry/opentelemetry-dotnet/tree/e330e57b04fa3e51fe5d63b52bfff891fb5b7961/src/OpenTelemetry.Exporter.Zipkin
                    tracing.AddZipkinExporter(x =>
                        x.Endpoint = new Uri(openTelemetryOptions.ZipkinOptions.HttpExporterEndpoint)
                    ); // "http://localhost:9411/api/v2/spans"
                }

                if (openTelemetryOptions.UseConsoleExporter)
                {
                    tracing.AddConsoleExporter();
                }

                optionsConfigurations.ConfigureTracerProvider?.Invoke(tracing);
            });
        }

        if (openTelemetryOptions.MetricsEnabled)
        {
            otel.WithMetrics(metrics =>
            {
                metrics
                    .AddMeter(instrumentationName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddProcessInstrumentation()
                    .AddRuntimeInstrumentation()
                    // metrics provides by ASP.NET Core in .NET 8
                    .AddView(
                        "http.server.request.duration",
                        new ExplicitBucketHistogramConfiguration
                        {
                            Boundaries = [0, 0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10],
                        }
                    );

                if (openTelemetryOptions.UsePrometheusExporter)
                {
                    // https://github.com/open-telemetry/opentelemetry-dotnet/tree/e330e57b04fa3e51fe5d63b52bfff891fb5b7961/src/OpenTelemetry.Exporter.Prometheus.AspNetCore
                    // https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Exporter.Prometheus.AspNetCore/README.md
                    // for exporting app metrics to `/metrics` endpoint
                    // http://localhost:5000/metrics
                    metrics.AddPrometheusExporter(o => o.DisableTotalNameSuffixForCounters = true);
                }

                if (openTelemetryOptions.UseConsoleExporter)
                {
                    metrics.AddConsoleExporter();
                }

                optionsConfigurations.ConfigureMeterProvider?.Invoke(metrics);
            });
        }

        builder.AddOpenTelemetryExporters(openTelemetryOptions);

        return builder;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(
        this TBuilder builder,
        OpenTelemetryOptions openTelemetryOptions
    )
        where TBuilder : IHostApplicationBuilder
    {
        var useOTLPGrpcExporter =
            !string.IsNullOrWhiteSpace(openTelemetryOptions.OTLPOptions.OTLPGrpcExporterEndpoint)
            && openTelemetryOptions.OTLPOptions.Enabled;
        var useOTLPHttpExporter =
            !string.IsNullOrWhiteSpace(openTelemetryOptions.OTLPOptions.OTLPHttpExporterEndpoint)
            && openTelemetryOptions.OTLPOptions.Enabled;
        var useAspireOTLPExporter =
            !string.IsNullOrWhiteSpace(openTelemetryOptions.AspireDashboardOTLPOptions.OTLPGrpcExporterEndpoint)
            && openTelemetryOptions.AspireDashboardOTLPOptions.Enabled;
        var useApplicationInsight =
            !string.IsNullOrWhiteSpace(openTelemetryOptions.ApplicationInsightOTLPOptions.ConnectionString)
            && openTelemetryOptions.ApplicationInsightOTLPOptions.Enabled;

        if (useOTLPGrpcExporter)
        {
            builder
                .Services.AddOpenTelemetry()
                .UseOtlpExporter(
                    OtlpExportProtocol.Grpc,
                    new Uri(openTelemetryOptions.OTLPOptions.OTLPGrpcExporterEndpoint!)
                );
        }

        if (useOTLPHttpExporter)
        {
            builder
                .Services.AddOpenTelemetry()
                .UseOtlpExporter(
                    OtlpExportProtocol.HttpProtobuf,
                    new Uri(openTelemetryOptions.OTLPOptions.OTLPHttpExporterEndpoint!)
                );
        }

        if (!string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
        {
            // https://github.com/open-telemetry/opentelemetry-dotnet/tree/main/src/OpenTelemetry.Exporter.OpenTelemetryProtocol#exporter-configuration
            // Use `UseOtlpExporter` internally use `OtlpExporterOptions` which set its properties if there are some environment variables like `OTEL_EXPORTER_OTLP_ENDPOINT`, `OTEL_EXPORTER_OTLP_PROTOCOL`
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        if (useAspireOTLPExporter)
        {
            // we can just one `AddOtlpExporter` and in development use `aspire-dashboard` OTLP endpoint address as `OTLPExporterEndpoint` and in production we can use `otel-collector` OTLP endpoint address
            builder
                .Services.AddOpenTelemetry()
                .UseOtlpExporter(
                    OtlpExportProtocol.Grpc,
                    new Uri(openTelemetryOptions.AspireDashboardOTLPOptions.OTLPGrpcExporterEndpoint!)
                );
        }

        if (openTelemetryOptions.UseGrafanaExporter)
        {
            // https://github.com/grafana/grafana-opentelemetry-dotnet/blob/main/docs/configuration.md#aspnet-core
            // https://github.com/grafana/grafana-opentelemetry-dotnet/
            // https://github.com/grafana/grafana-opentelemetry-dotnet/blob/main/docs/configuration.md#sending-to-an-agent-or-collector-via-otlp
            // https://grafana.com/docs/grafana-cloud/monitor-applications/application-observability/instrument/dotnet/
            builder.Services.AddOpenTelemetry().UseGrafana();
        }

        // enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
        if (useApplicationInsight)
        {
            builder
                .Services.AddOpenTelemetry()
                .UseAzureMonitor(x =>
                {
                    x.ConnectionString = openTelemetryOptions.ApplicationInsightOTLPOptions.ConnectionString;
                });
        }

        return builder;
    }
}
