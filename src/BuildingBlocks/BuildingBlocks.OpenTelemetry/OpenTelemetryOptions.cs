namespace BuildingBlocks.OpenTelemetry;

public class OpenTelemetryOptions
{
    public string? ServiceName { get; set; }
    public bool UsePrometheusExporter { get; set; } = true;
    public bool UseGrafanaExporter { get; set; }
    public bool UseConsoleExporter { get; set; }
    public ZipkinOptions? ZipkinOptions { get; set; } = default!;
    public JaegerOptions? JaegerOptions { get; set; } = default!;
    public OpenTelemetryCollectorOptions? OpenTelemetryCollectorOptions { get; set; } = default!;
    public AspireDashboardOTLPOptions? AspireDashboardOTLPOptions { get; set; } = default!;
    public ApplicationInsightOTLPOptions? ApplicationInsightOTLPOptions { get; set; } = default!;
}

// https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Exporter.Zipkin/README.md
public class ZipkinOptions
{
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets endpoint address to receive telemetry
    /// </summary>
    public string? HttpExporterEndpoint { get; set; } = "http://localhost:9411/api/v2/spans";
}

public class JaegerOptions
{
    public bool Enabled { get; set; }
    public string? OTLPGrpcExporterEndpoint { get; set; } = "http://localhost:14317";
    public string? HttpExporterEndpoint { get; set; } = "http://localhost:14268/api/traces";
}

public class OpenTelemetryCollectorOptions
{
    public bool Enabled { get; set; } = true;
    public string? OTLPGrpcExporterEndpoint { get; set; } = "http://localhost:4317";
    public string? OTLPHttpExporterEndpoint { get; set; } = "http://localhost:4318";
}

public class AspireDashboardOTLPOptions
{
    public bool Enabled { get; set; } = true;
    public string? OTLPGrpcExporterEndpoint { get; set; } = "http://localhost:4319";
}

public class ApplicationInsightOTLPOptions
{
    public bool Enabled { get; set; }
    public string? ConnectionString { get; set; }
}
