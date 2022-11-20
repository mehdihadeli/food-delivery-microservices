namespace BuildingBlocks.OpenTelemetry;

public enum TracingExporterType
{
    Console = 0,
    OTLP = 1,
    Jaeger = 2,
    Zipkin = 3,
    None = 4,
}

public enum MetricsExporterType
{
    Console = 0,
    OTLP = 1,
    None = 4,
}

public enum LogExporterType
{
    Console = 0,
    OTLP = 1,
    None = 4,
}
