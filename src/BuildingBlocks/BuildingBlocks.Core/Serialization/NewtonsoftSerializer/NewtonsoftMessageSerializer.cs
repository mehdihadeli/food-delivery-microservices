using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Abstractions.Serialization;
using BuildingBlocks.Core.Messages;
using Newtonsoft.Json;

namespace BuildingBlocks.Core.Serialization.NewtonsoftSerializer;

public class NewtonsoftMessageSerializer(JsonSerializerSettings settings) : IMessageSerializer
{
    public string ContentType => "application/json";

    public string Serialize(IMessageEnvelopeBase iMessageEnvelope)
    {
        return JsonConvert.SerializeObject(iMessageEnvelope, settings);
    }

    public string Serialize<T>(IMessageEnvelope<T> messageEnvelope)
        where T : class, IMessage
    {
        return JsonConvert.SerializeObject(messageEnvelope, settings);
    }

    public IMessageEnvelopeBase? Deserialize(string eventEnvelope, Type? messageType)
    {
        // Get the generic type definition of MessageEnvelopeFactory
        Type eventEnvelopeType = typeof(MessageEnvelope<>);
        Type eventEnvelopGenericType = eventEnvelopeType.MakeGenericType(messageType);

        return JsonConvert.DeserializeObject(eventEnvelope, eventEnvelopGenericType, settings) as IMessageEnvelopeBase;
    }

    public IMessageEnvelope<T>? Deserialize<T>(string eventEnvelope)
        where T : class, IMessage
    {
        return JsonConvert.DeserializeObject<MessageEnvelope<T>>(eventEnvelope, settings);
    }
}
