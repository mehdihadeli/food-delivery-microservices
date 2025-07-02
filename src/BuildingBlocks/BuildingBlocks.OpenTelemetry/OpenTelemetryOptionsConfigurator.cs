using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace BuildingBlocks.OpenTelemetry;

public class OpenTelemetryOptionsConfigurator
{
    public Action<TracerProviderBuilder>? ConfigureTracerProvider { get; private set; }
    public Action<MeterProviderBuilder>? ConfigureMeterProvider { get; private set; }

    public OpenTelemetryOptionsConfigurator ConfigureTracing(Action<TracerProviderBuilder> configure)
    {
        ConfigureTracerProvider = configure;

        return this;
    }

    public OpenTelemetryOptionsConfigurator ConfigureMetrics(Action<MeterProviderBuilder> configure)
    {
        ConfigureMeterProvider = configure;

        return this;
    }
}
