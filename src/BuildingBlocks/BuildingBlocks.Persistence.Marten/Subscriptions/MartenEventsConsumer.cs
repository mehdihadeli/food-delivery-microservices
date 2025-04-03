using System.Diagnostics;
using BuildingBlocks.Abstractions.Core.Diagnostics;
using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Core.Diagnostics;
using BuildingBlocks.Persistence.Marten.Extensions;
using Marten;
using Marten.Events;
using Shared.Observability;

namespace BuildingBlocks.Persistence.Marten.Subscriptions;

public class MartenEventsConsumer(IInternalEventBus internalEventBus, IDiagnosticsProvider diagnosticsProvider)
    : IMartenEventsConsumer
{
    public async Task ConsumeAsync(
        IDocumentOperations documentOperations,
        IReadOnlyList<StreamAction> streamActions,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var @event in streamActions.SelectMany(streamAction => streamAction.Events))
        {
            var parentContext = TelemetryPropagator.Extract(@event.Headers, ExtractTraceContextFromEventMetadata);

            var activityName =
                $"{DiagnosticsConstant.Components.Consumer}.{nameof(MartenEventsConsumer)}/{nameof(ConsumeAsync)}";

            await diagnosticsProvider
                .ExecuteActivityAsync(
                    new CreateActivityInfo
                    {
                        Name = activityName,
                        ActivityKind = ActivityKind.Consumer,
                        Tags =
                        {
                            { TelemetryTags.Tracing.Event.Type, @event.Data.GetType() },
                            { TelemetryTags.Tracing.Event.Name, @event.Data.GetType().Name },
                        },
                        Parent = parentContext.ActivityContext,
                    },
                    async (_, ct) =>
                    {
                        var streamEvent = @event.ToStreamEvent(parentContext);

                        await internalEventBus.Publish(streamEvent, ct).ConfigureAwait(false);
                    },
                    cancellationToken
                )
                .ConfigureAwait(false);
        }
    }

    private IEnumerable<string> ExtractTraceContextFromEventMetadata(Dictionary<string, object>? headers, string key)
    {
        if (headers!.TryGetValue(key, out var value) != true)
            return [];

        var stringValue = value.ToString();

        return stringValue != null ? [stringValue] : [];
    }
}
