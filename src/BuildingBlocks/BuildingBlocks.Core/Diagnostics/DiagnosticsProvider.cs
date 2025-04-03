using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;
using BuildingBlocks.Abstractions.Core.Diagnostics;
using BuildingBlocks.Core.Diagnostics.Extensions;

namespace BuildingBlocks.Core.Diagnostics;

public class DiagnosticsProvider(IMeterFactory meterFactory) : IDiagnosticsProvider
{
    private readonly Version? _version = Assembly.GetCallingAssembly().GetName().Version;
    private ActivitySource? _activitySource;
    private ActivityListener? _listener;
    private Meter? _meter;

    public string InstrumentationName { get; } = DiagnosticsConstant.ApplicationInstrumentationName;

    // https://learn.microsoft.com/en-us/dotnet/core/diagnostics/distributed-tracing-instrumentation-walkthroughs
    // ActivitySource is like a tracer object for creating Activity (spans)
    public ActivitySource ActivitySource
    {
        get
        {
            if (_activitySource != null)
                return _activitySource;

            _activitySource = new(InstrumentationName, _version?.ToString());

            _listener = new ActivityListener
            {
                ShouldListenTo = x => true,
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            };
            ActivitySource.AddActivityListener(_listener);

            return _activitySource;
        }
    }

    // https://learn.microsoft.com/en-us/dotnet/core/diagnostics/metrics-instrumentation
    public Meter Meter
    {
        get
        {
            if (_meter != null)
                return _meter;

            _meter = meterFactory.Create(InstrumentationName, _version?.ToString());

            return _meter;
        }
    }

    public async Task ExecuteActivityAsync(
        CreateActivityInfo createActivityInfo,
        Func<Activity?, CancellationToken, Task> action,
        CancellationToken cancellationToken = default
    )
    {
        if (_activitySource != null && !_activitySource.HasListeners())
        {
            await action(null, cancellationToken);

            return;
        }

        using var activity =
            ActivitySource
                .CreateActivity(
                    name: $"{InstrumentationName}.{createActivityInfo.Name}",
                    kind: createActivityInfo.ActivityKind,
                    parentContext: createActivityInfo.Parent ?? default,
                    idFormat: ActivityIdFormat.W3C,
                    tags: createActivityInfo.Tags
                )
                ?.Start() ?? Activity.Current;

        try
        {
            await action(activity!, cancellationToken);
            activity?.SetOkStatus();
        }
        catch (System.Exception ex)
        {
            activity?.SetErrorStatus(ex);
            throw;
        }
    }

    public async Task<TResult?> ExecuteActivityAsync<TResult>(
        CreateActivityInfo createActivityInfo,
        Func<Activity?, CancellationToken, Task<TResult>> action,
        CancellationToken cancellationToken = default
    )
    {
        if (_activitySource != null && !_activitySource.HasListeners())
        {
            return await action(null, cancellationToken);
        }

        using var activity =
            ActivitySource
                .CreateActivity(
                    name: $"{InstrumentationName}.{createActivityInfo.Name}",
                    kind: createActivityInfo.ActivityKind,
                    parentContext: createActivityInfo.Parent ?? default,
                    idFormat: ActivityIdFormat.W3C,
                    tags: createActivityInfo.Tags
                )
                ?.Start() ?? Activity.Current;

        try
        {
            var result = await action(activity!, cancellationToken);

            activity?.SetOkStatus();

            return result;
        }
        catch (System.Exception ex)
        {
            activity?.SetErrorStatus(ex);
            throw;
        }
    }

    public void Dispose()
    {
        _listener?.Dispose();
        _meter?.Dispose();
        _activitySource?.Dispose();
    }
}
