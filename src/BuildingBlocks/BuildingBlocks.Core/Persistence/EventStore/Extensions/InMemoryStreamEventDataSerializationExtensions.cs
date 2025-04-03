using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Core.Messages;

namespace BuildingBlocks.Core.Persistence.EventStore.Extensions;

using System.Text;
using BuildingBlocks.Abstractions.Persistence.EventStore;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Types;
using Newtonsoft.Json;

public static class InMemoryStreamEventDataSerializationExtensions
{
    public static T DeserializeData<T>(this StreamEventData resolvedEvent) => (T)DeserializeData(resolvedEvent);

    public static object DeserializeData(this StreamEventData eventData)
    {
        eventData.NotBeNull();

        // get type
        var eventType = TypeMapper.GetType(eventData.EventType);

        // deserialize event
        return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(eventData.Data), eventType)!;
    }

    public static StreamEventMetadata? DeserializeMetadata(this StreamEventData eventData)
    {
        eventData.NotBeNull();

        return eventData.Metadata is null
            ? null
            : JsonConvert.DeserializeObject<StreamEventMetadata>(Encoding.UTF8.GetString(eventData.Metadata))!;
    }

    public static IStreamEventEnvelopeBase ToStreamEvent(this StreamEventData streamEventData)
    {
        var eventData = streamEventData.DeserializeData();
        var metaData = streamEventData.DeserializeMetadata();

        return StreamEventEnvelopeFactory.From(eventData, metaData);
    }

    public static StreamEventData ToJsonStreamEventData(this IStreamEventEnvelopeBase @event)
    {
        @event.NotBeNull();

        return ToJsonStreamEventData(@event.Data, @event.Metadata);
    }

    public static StreamEventData ToJsonStreamEventData(this object @event, StreamEventMetadata? metadata = null)
    {
        return new StreamEventData
        {
            EventId = Guid.NewGuid(),
            EventType = TypeMapper.AddFullTypeName(@event.GetType()),
            Data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event)),
            Metadata = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(metadata ?? new object())),
            ContentType = "application/json",
        };
    }
}
