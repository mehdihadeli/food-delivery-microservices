using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.OpenTelemetry.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication MapDefaultOpenTelemetry(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<OpenTelemetryOptions>>().Value;

        if (options.UsePrometheusExporter)
        {
            // http://localhost:5000/metrics
            // to export health metrics in `/metrics` endpoint and should scrape in the Prometheus config file and `scrape_configs` besides of `/metrics` endpoint for application metrics
            // requires the OpenTelemetry.Exporter.Prometheus.AspNetCore package
            app.MapPrometheusScrapingEndpoint();
        }

        return app;
    }
}
