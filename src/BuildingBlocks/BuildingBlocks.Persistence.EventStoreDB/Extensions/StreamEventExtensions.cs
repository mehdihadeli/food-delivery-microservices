using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Abstractions.Persistence.EventStore;
using EventStore.Client;

namespace BuildingBlocks.Persistence.EventStoreDB.Extensions;

public static class StreamEventExtensions
{
    public static IEnumerable<IStreamEventEnvelopeBase> ToStreamEvents(this IEnumerable<ResolvedEvent> resolvedEvents)
    {
        return resolvedEvents.Select(x => x.ToStreamEvent());
    }

    // public static IStreamEvent? ToStreamEvent(this ResolvedEvent resolvedEvent)
    // {
    //     var eventData = resolvedEvent.Deserialize();
    //     var eventMetadata = resolvedEvent.DeserializePropagationContext();
    //
    //     if (eventData == null)
    //         return null;
    //
    //     var metaData = new StreamEventMetadata(
    //         resolvedEvent.Event.EventId.ToString(),
    //         resolvedEvent.Event.EventNumber.ToUInt64(),
    //         resolvedEvent.Event.Position.CommitPosition,
    //         eventMetadata
    //     );
    //
    //     return StreamEventFactory.From(eventData, metaData);
    // }
}
