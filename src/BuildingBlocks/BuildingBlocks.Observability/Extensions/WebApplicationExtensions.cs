using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using Shared.Observability;

namespace BuildingBlocks.Observability.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseObservability(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<ObservabilityOptions>>().Value;

        app.Use(
            async (context, next) =>
            {
                var metricsFeature = context.Features.Get<IHttpMetricsTagsFeature>();
                if (metricsFeature != null && context.Request.Path is { Value: "/metrics" or "/health" })
                {
                    metricsFeature.MetricsDisabled = true;
                }

                await next(context);
            }
        );

        if (options.UsePrometheusExporter)
        {
            // export application metrics in `/metrics` endpoint and should scrape in the Prometheus config file and `scrape_configs`
            // https://github.com/open-telemetry/opentelemetry-dotnet/tree/e330e57b04fa3e51fe5d63b52bfff891fb5b7961/src/OpenTelemetry.Exporter.Prometheus.AspNetCore
            // https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Exporter.Prometheus.AspNetCore/README.md
            // http://localhost:5000/metrics
            app.UseOpenTelemetryPrometheusScrapingEndpoint();
        }

        return app;
    }
}
