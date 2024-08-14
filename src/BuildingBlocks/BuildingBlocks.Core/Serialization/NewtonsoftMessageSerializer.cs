using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Serialization;
using BuildingBlocks.Core.Events;
using Newtonsoft.Json;

namespace BuildingBlocks.Core.Serialization;

public class NewtonsoftMessageSerializer(JsonSerializerSettings settings) : IMessageSerializer
{
    public string ContentType => "application/json";

    public string Serialize(IEventEnvelope eventEnvelope)
    {
        return JsonConvert.SerializeObject(eventEnvelope, settings);
    }

    public IEventEnvelope? Deserialize(string eventEnvelope)
    {
        return JsonConvert.DeserializeObject<EventEnvelope<object>>(eventEnvelope, settings);
    }

    public IEventEnvelope? Deserialize(string eventEnvelope, Type messageType)
    {
        // Get the generic type definition of EventEnvelope
        Type eventEnvelopeType = typeof(EventEnvelope<>);
        Type eventEnvelopGenericType = eventEnvelopeType.MakeGenericType(messageType);

        return JsonConvert.DeserializeObject(eventEnvelope, eventEnvelopGenericType, settings) as IEventEnvelope;
    }
}
