using System.Text.Json;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Serialization;
using BuildingBlocks.Core.Types;

namespace BuildingBlocks.Core.Serialization;

public class DefaultMessageSerializer : IMessageSerializer
{
    private readonly JsonSerializerOptions? _settings = new() {WriteIndented = true};

    public string ContentType => "application/json";

    public string Serialize(MessageEnvelope messageEnvelope)
    {
        return JsonSerializer.Serialize(messageEnvelope, _settings);
    }

    public string Serialize<TMessage>(TMessage message)
        where TMessage : IMessage
    {
        return JsonSerializer.Serialize(message, _settings);
    }

    public TMessage? Deserialize<TMessage>(string message)
        where TMessage : IMessage
    {
        return JsonSerializer.Deserialize<TMessage>(message, _settings);
    }

    public object? Deserialize(string payload, string payloadType)
    {
        var type = TypeMapper.GetType(payloadType);
        var deserializedData = JsonSerializer.Deserialize(payload, type, _settings);

        return deserializedData;
    }

    public MessageEnvelope? Deserialize(string json)
    {
        return JsonSerializer.Deserialize<MessageEnvelope>(json, _settings);
    }

    public IMessage? Deserialize(ReadOnlySpan<byte> data, string payloadType)
    {
        var type = TypeMapper.GetType(payloadType);

        var deserializedData = JsonSerializer.Deserialize(data, type, _settings);

        return deserializedData as IMessage;
    }
}
