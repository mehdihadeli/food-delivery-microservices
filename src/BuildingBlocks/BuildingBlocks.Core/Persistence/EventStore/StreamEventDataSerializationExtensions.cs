using System.Text;
using BuildingBlocks.Abstractions.CQRS.Events.Internal;
using BuildingBlocks.Abstractions.Persistence.EventStore;
using BuildingBlocks.Core.Types;
using Newtonsoft.Json;

namespace BuildingBlocks.Core.Persistence.EventStore;

public static class StreamEventDataSerializationExtensions
{
    public static T DeserializeData<T>(this StreamEventData resolvedEvent) => (T)DeserializeData(resolvedEvent);

    public static object DeserializeData(this StreamEventData eventData)
    {
        // get type
        var eventType = TypeMapper.GetType(eventData.EventType);

        // deserialize event
        return JsonConvert.DeserializeObject(
            Encoding.UTF8.GetString(eventData.Data),
            eventType)!;
    }

    public static IStreamEventMetadata? DeserializeMetadata(this StreamEventData eventData)
    {
        if (eventData.Metadata is null)
            return null;

        // deserialize event
        return JsonConvert.DeserializeObject<StreamEventMetadata>(
            Encoding.UTF8.GetString(eventData.Metadata))!;
    }

    public static StreamEvent ToStreamEvent(this StreamEventData streamEventData)
    {
        var eventData = streamEventData.DeserializeData();
        var metaData = streamEventData.DeserializeMetadata();

        var type = typeof(StreamEvent<>).MakeGenericType(eventData.GetType());

        return (StreamEvent)Activator.CreateInstance(
            type,
            eventData,
            metaData)!;
    }

    public static StreamEventData ToJsonStreamEventData(this IStreamEvent @event)
    {
        return ToJsonStreamEventData(@event.Data, @event.Metadata);
    }

    public static StreamEventData ToJsonStreamEventData(this IDomainEvent @event, IStreamEventMetadata? metadata = null)
    {
        return new StreamEventData
        {
            EventId = Guid.NewGuid(),
            EventType = TypeMapper.GetFullTypeNameByObject(@event),
            Data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event)),
            Metadata = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(metadata ?? new object())),
            ContentType = "application/json"
        };
    }
}
