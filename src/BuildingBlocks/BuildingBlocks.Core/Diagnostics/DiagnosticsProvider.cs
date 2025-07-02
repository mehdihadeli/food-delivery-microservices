using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;
using BuildingBlocks.Abstractions.Core.Diagnostics;
using BuildingBlocks.Core.Extensions;

namespace BuildingBlocks.Core.Diagnostics;

public class DiagnosticsProvider(IMeterFactory meterFactory, string instrumentationName) : IDiagnosticsProvider
{
    private readonly string? _version = Assembly.GetCallingAssembly().GetName().Version?.ToString();
    private readonly List<ActivityListener> _customListeners = new();

    private ActivitySource? _activitySource;
    private Meter? _meter;

    public string InstrumentationName { get; } = instrumentationName.NotBeEmptyOrNull();
    public ActivitySource ActivitySource => _activitySource ??= new ActivitySource(InstrumentationName, _version);

    public Meter Meter => _meter ??= meterFactory.Create(InstrumentationName, _version);

    public void AddCustomActivityListener(ActivityListener listener)
    {
        ArgumentNullException.ThrowIfNull(listener);

        _customListeners.Add(listener);

        ActivitySource.AddActivityListener(listener);
    }

    public void AddEmptyListener()
    {
        var emptyListener = new ActivityListener
        {
            // Listen to all sources
            ShouldListenTo = source => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> options) =>
                ActivitySamplingResult.AllDataAndRecorded,
            ActivityStarted = activity =>
            {
                activity.AddTag("service.name", InstrumentationName);
                Console.WriteLine($"Activity Started: {activity}");
            },
            ActivityStopped = activity =>
            {
                activity.AddTag("service.name", InstrumentationName);
                Console.WriteLine($"Activity Stopped: {activity}");
            },
        };

        _customListeners.Add(emptyListener);

        ActivitySource.AddActivityListener(emptyListener);
    }

    // Dispose of all listeners and other resources
    public void Dispose()
    {
        foreach (var listener in _customListeners)
        {
            listener.Dispose();
        }

        _activitySource?.Dispose();
        _meter?.Dispose();
    }
}
