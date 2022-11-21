namespace BuildingBlocks.OpenTelemetry;

public class OpenTelemetryOptions
{
    public string TracingExporterType { get; set; } = nameof(OpenTelemetry.TracingExporterType.None);
    public string LogExporterType { get; set; } = nameof(OpenTelemetry.LogExporterType.None);
    public string MetricsExporterType { get; set; } = nameof(OpenTelemetry.MetricsExporterType.None);
    public JaegerExporterOptions JaegerOptions { get; set; } = default!;
    public ZipkinExporterOptions ZipkinOptions { get; set; } = default!;
    public OTLPExporterOptions OTLPOptions { get; set; } = default!;
}

public class JaegerExporterOptions
{
    public string AgentHost { get; set; } = default!;
    public int AgentPort { get; set; } = default!;
}

public class ZipkinExporterOptions
{
    public string Endpoint { get; set; } = default!;
}

public class OTLPExporterOptions
{
    public string OTLPEndpoint { get; set; } = default!;
}
