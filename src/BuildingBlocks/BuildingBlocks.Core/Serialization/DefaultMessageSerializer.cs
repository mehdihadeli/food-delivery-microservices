using System.Text.Json;
using Ardalis.GuardClauses;
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

    public TransportMessage SerializeToTransportMessage(MessageEnvelope messageEnvelope)
    {
        Guard.Against.Null(messageEnvelope, nameof(messageEnvelope));

        if (messageEnvelope.Message == null)
        {
            return new TransportMessage(null, messageEnvelope.Headers);
        }

        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(messageEnvelope.Message, _settings);
        return new TransportMessage(jsonBytes, messageEnvelope.Headers);
    }

    public TMessage? Deserialize<TMessage>(string message)
        where TMessage : IMessage
    {
        return JsonSerializer.Deserialize<TMessage>(message, _settings);
    }

    public MessageEnvelope? Deserialize(string json)
    {
        return JsonSerializer.Deserialize<MessageEnvelope>(json, _settings);
    }

    public MessageEnvelope Deserialize(TransportMessage transportMessage, Type? payloadType)
    {
        if (payloadType == null || transportMessage.Body == null || transportMessage.Body.Length == 0)
        {
            return new MessageEnvelope(null, transportMessage.Headers);
        }

        var obj = JsonSerializer.Deserialize(transportMessage.Body, payloadType, _settings) as IMessage;

        return new MessageEnvelope(obj, transportMessage.Headers);
    }

    public IMessage? Deserialize(ReadOnlySpan<byte> data, string payloadType)
    {
        var type = TypeMapper.GetType(payloadType);

        var deserializedData = JsonSerializer.Deserialize(data, type, _settings);

        return deserializedData as IMessage;
    }
}
