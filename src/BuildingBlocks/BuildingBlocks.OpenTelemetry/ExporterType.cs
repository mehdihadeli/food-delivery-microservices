namespace BuildingBlocks.OpenTelemetry;

public enum ExporterType
{
    Console = 0,
    OTLP = 1,
    Jaeger = 2,
    Zipkin = 3,
    None = 4,
}
