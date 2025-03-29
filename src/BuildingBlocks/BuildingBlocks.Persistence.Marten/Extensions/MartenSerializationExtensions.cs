using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Abstractions.Persistence.EventStore;
using BuildingBlocks.Core.Messages;
using BuildingBlocks.Core.Persistence.EventStore;
using Marten.Events;
using OpenTelemetry.Context.Propagation;

namespace BuildingBlocks.Persistence.Marten.Extensions;

public static class MartenSerializationExtensions
{
    public static IStreamEventEnvelopeBase ToStreamEvent(
        this IEvent resolvedEvent,
        PropagationContext? propagationContext = null
    )
    {
        var metaData = resolvedEvent.DeserializeMetadata(propagationContext);

        return StreamEventEnvelopeFactory.From(resolvedEvent.Data, metaData);
    }

    public static StreamEventMetadata DeserializeMetadata(
        this IEvent resolvedEvent,
        PropagationContext? propagationContext = null
    )
    {
        var eventMetadata = new StreamEventMetadata(
            resolvedEvent.Id.ToString(),
            (ulong)resolvedEvent.Version,
            (ulong)resolvedEvent.Sequence,
            propagationContext
        );

        return eventMetadata;
    }
}
