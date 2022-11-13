namespace BuildingBlocks.OpenTelemetry;

public class OpenTelemetryOptions
{
    public string ServiceName { get; set; } = default!;
    public string TracingExporterType { get; set; } = default!;
    public string LogExporterType { get; set; } = default!;
    public string MetricsExporterType { get; set; } = default!;
    public string OTLPEndpoint { get; set; } = default!;
}
