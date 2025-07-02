using System.Diagnostics;
using BuildingBlocks.Abstractions.Core.Diagnostics;
using BuildingBlocks.Core.Diagnostics.Extensions;
using BuildingBlocks.Core.Extensions;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Core.Diagnostics;

public class ActivityRunner(IDiagnosticsProvider diagnosticsProvider, ILogger<ActivityRunner> logger) : IActivityRunner
{
    public Activity? CreateAndStartActivity(CreateActivityInfo createActivityInfo)
    {
        ArgumentNullException.ThrowIfNull(createActivityInfo);

        // ActivitySource.CreateActivity() returns null when we don't have any listener like open-telemetry
        var activity = createActivityInfo.Parent is not null
            ? diagnosticsProvider.ActivitySource.CreateActivity(
                name: $"{diagnosticsProvider.InstrumentationName}.{createActivityInfo.Name}",
                kind: createActivityInfo.ActivityKind,
                parentContext: createActivityInfo.Parent.Value,
                idFormat: ActivityIdFormat.W3C,
                tags: createActivityInfo.Tags
            )
            : diagnosticsProvider.ActivitySource.CreateActivity(
                name: $"{diagnosticsProvider.InstrumentationName}.{createActivityInfo.Name}",
                kind: createActivityInfo.ActivityKind,
                parentId: createActivityInfo.ParentId ?? Activity.Current?.ParentId,
                idFormat: ActivityIdFormat.W3C,
                tags: createActivityInfo.Tags
            );

        return activity?.Start();
    }

    public async Task ExecuteActivityAsync(
        CreateActivityInfo createActivityInfo,
        Func<Activity?, CancellationToken, Task> action,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(createActivityInfo);

        using var activity = CreateAndStartActivity(createActivityInfo);

        try
        {
            await action(activity, cancellationToken);
            activity?.SetOkStatus();
        }
        catch (System.Exception ex)
        {
            HandleActivityError(activity, ex);
            throw;
        }
    }

    public async Task<TResult?> ExecuteActivityAsync<TResult>(
        CreateActivityInfo createActivityInfo,
        Func<Activity?, CancellationToken, Task<TResult>> action,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(createActivityInfo);

        using var activity = CreateAndStartActivity(createActivityInfo);

        try
        {
            var result = await action(activity, cancellationToken);
            activity?.SetOkStatus();
            return result;
        }
        catch (System.Exception ex)
        {
            HandleActivityError(activity, ex);
            throw;
        }
    }

    private void HandleActivityError(Activity? activity, System.Exception ex)
    {
        activity?.SetErrorStatus(ex);
        logger.LogError("Error occurred during activity {ActivityDisplayName}: {Exception}", activity?.DisplayName, ex);
    }
}
