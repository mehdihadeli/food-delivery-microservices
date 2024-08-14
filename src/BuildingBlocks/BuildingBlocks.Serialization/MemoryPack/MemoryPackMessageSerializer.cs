using System.Text;
using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Serialization;
using BuildingBlocks.Core.Events;
using MemoryPack;
using Type = System.Type;

namespace BuildingBlocks.Serialization.MemoryPack;

public class MemoryPackMessageSerializer(MemoryPackSerializerOptions options) : IMessageSerializer
{
    public string ContentType => "binary/memorypack";

    public string Serialize(IEventEnvelope eventEnvelope)
    {
        return Encoding.UTF8.GetString(MemoryPackSerializer.Serialize(eventEnvelope, options));
    }

    public IEventEnvelope? Deserialize(string eventEnvelope)
    {
        ReadOnlySpan<byte> byteSpan = StringToReadOnlySpan(eventEnvelope);

        return MemoryPackSerializer.Deserialize<EventEnvelope<object>>(byteSpan, options);
    }

    public IEventEnvelope? Deserialize(string eventEnvelope, Type messageType)
    {
        // Get the generic type definition of EventEnvelope
        Type eventEnvelopeType = typeof(EventEnvelope<>);
        Type eventEnvelopGenericType = eventEnvelopeType.MakeGenericType(messageType);

        ReadOnlySpan<byte> byteSpan = StringToReadOnlySpan(eventEnvelope);

        return MemoryPackSerializer.Deserialize(eventEnvelopGenericType, byteSpan, options) as IEventEnvelope;
    }

    private static ReadOnlySpan<byte> StringToReadOnlySpan(string input)
    {
        // Choose the encoding
        Encoding encoding = Encoding.UTF8;

        // Convert the string to a byte array
        byte[] byteArray = encoding.GetBytes(input);

        // Return a ReadOnlySpan<byte> from the byte array
        return new ReadOnlySpan<byte>(byteArray);
    }
}
