using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Shared.Observability;

public class ObservabilityOptionsConfigurator
{
    public Action<TracerProviderBuilder>? ConfigureTracerProvider { get; private set; }
    public Action<MeterProviderBuilder>? ConfigureMeterProvider { get; private set; }

    public ObservabilityOptionsConfigurator ConfigureTracing(Action<TracerProviderBuilder> configure)
    {
        ConfigureTracerProvider = configure;

        return this;
    }

    public ObservabilityOptionsConfigurator ConfigureMetrics(Action<MeterProviderBuilder> configure)
    {
        ConfigureMeterProvider = configure;

        return this;
    }
}
