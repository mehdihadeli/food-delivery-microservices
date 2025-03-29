using System.Text;
using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Abstractions.Persistence.EventStore;
using BuildingBlocks.Core.Messages;
using BuildingBlocks.Core.Persistence.EventStore;
using BuildingBlocks.Core.Types;
using EventStore.Client;
using Newtonsoft.Json;
using OpenTelemetry.Context.Propagation;

namespace BuildingBlocks.Persistence.EventStoreDB.Extensions;

public static class EventStoreDbSerializationExtensions
{
    public static T DeserializeData<T>(this ResolvedEvent resolvedEvent) => (T)DeserializeData(resolvedEvent);

    public static object DeserializeData(this ResolvedEvent resolvedEvent)
    {
        // get type
        var eventType = TypeMapper.GetType(resolvedEvent.Event.EventType);

        // deserialize event
        return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span), eventType)!;
    }

    public static StreamEventMetadata DeserializeMetadata(
        this ResolvedEvent resolvedEvent,
        PropagationContext? propagationContext = null
    )
    {
        // deserialize event
        var streamEventMetadata = JsonConvert.DeserializeObject<StreamEventMetadata>(
            Encoding.UTF8.GetString(resolvedEvent.Event.Metadata.Span)
        )!;

        return streamEventMetadata with
        {
            PropagationContext = propagationContext,
        };
    }

    public static EventData ToJsonEventData(this IStreamEventEnvelopeBase @event)
    {
        return ToJsonEventData(@event.Data, @event.Metadata);
    }

    public static EventData ToJsonEventData(this object @event, StreamEventMetadata? metadata = null)
    {
        return new(
            Uuid.NewUuid(),
            TypeMapper.AddFullTypeName(@event.GetType()),
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event)),
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(metadata ?? new object()))
        );
    }

    public static IStreamEventEnvelopeBase ToStreamEvent(
        this ResolvedEvent resolvedEvent,
        PropagationContext? propagationContext = null
    )
    {
        var eventData = resolvedEvent.DeserializeData();
        var metaData = resolvedEvent.DeserializeMetadata(propagationContext);

        return StreamEventEnvelopeFactory.From(eventData, metaData);
    }
}
