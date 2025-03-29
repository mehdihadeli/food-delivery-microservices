using System.Text;
using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Abstractions.Serialization;
using BuildingBlocks.Core.Messages;
using MemoryPack;
using Type = System.Type;

namespace BuildingBlocks.Serialization.MemoryPack;

public class MemoryPackMessageSerializer(MemoryPackSerializerOptions options) : IMessageSerializer
{
    public string ContentType => "binary/memorypack";

    public string Serialize(IMessageEnvelopeBase iMessageEnvelope)
    {
        return Encoding.UTF8.GetString(MemoryPackSerializer.Serialize(iMessageEnvelope, options));
    }

    public string Serialize<T>(IMessageEnvelope<T> messageEnvelope)
        where T : class, IMessage
    {
        return Encoding.UTF8.GetString(MemoryPackSerializer.Serialize(messageEnvelope, options));
    }

    public IMessageEnvelopeBase? Deserialize(string eventEnvelope, Type? messageType)
    {
        // Get the generic type definition of MessageEnvelopeFactory
        Type eventEnvelopeType = typeof(MessageEnvelope<>);
        Type eventEnvelopGenericType = eventEnvelopeType.MakeGenericType(messageType);

        ReadOnlySpan<byte> byteSpan = StringToReadOnlySpan(eventEnvelope);

        return MemoryPackSerializer.Deserialize(eventEnvelopGenericType, byteSpan, options) as IMessageEnvelopeBase;
    }

    public IMessageEnvelope<T>? Deserialize<T>(string eventEnvelope)
        where T : class, IMessage
    {
        ReadOnlySpan<byte> byteSpan = StringToReadOnlySpan(eventEnvelope);

        return MemoryPackSerializer.Deserialize<MessageEnvelope<T>>(byteSpan, options);
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
