using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Core.Messaging.Extensions;
using BuildingBlocks.Core.Types;
using BuildingBlocks.Core.Types.Extensions;
using Humanizer;
using MessageHeaders = BuildingBlocks.Core.Messaging.MessageHeaders;

namespace BuildingBlocks.Core.Events;

internal record EventEnvelope<T>(T Data, IEventEnvelopeMetadata Metadata) : IEventEnvelope<T>
    where T : class
{
    object IEventEnvelope.Data => Data;
}

internal record EventEnvelope(object Data, IEventEnvelopeMetadata Metadata) : IEventEnvelope;

public static class EventEnvelopeFactory
{
    public static IEventEnvelope From(object data, IEventEnvelopeMetadata metadata)
    {
        //TODO: Get rid of reflection!
        var type = typeof(EventEnvelope<>).MakeGenericType(data.GetType());
        return (IEventEnvelope)Activator.CreateInstance(type, data, metadata)!;
    }

    public static IEventEnvelope From<TMessage>(TMessage data, IEventEnvelopeMetadata metadata)
        where TMessage : class, IMessage
    {
        if (!metadata.Headers.ContainsKey(MessageHeaders.MessageId))
        {
            var messageId = data.MessageId;
            metadata.Headers.AddMessageId(messageId);
        }

        if (!metadata.Headers.ContainsKey(MessageHeaders.CorrelationId))
        {
            // correlation will generate in the start point and will transfer with header propagation in api-gateway.
            metadata.Headers.AddCorrelationId(metadata.CorrelationId);
        }

        metadata.Headers.AddMessageName(metadata.Name);
        metadata.Headers.AddMessageType(metadata.MessageType);
        metadata.Headers.AddCreatedUnixTime(
            metadata.CreatedUnixTime ?? DateTimeExtensions.ToUnixTimeSecond(DateTime.Now)
        );

        return new EventEnvelope<TMessage>(data, metadata);
    }

    public static IEventEnvelope From<TMessage>(TMessage data, Guid correlationId)
        where TMessage : class, IMessage
    {
        var envelopeMetadata = new EventEnvelopeMetadata(
            data.MessageId,
            correlationId,
            TypeMapper.GetTypeName(data.GetType()),
            data.GetType().Name.Underscore()
        )
        {
            CreatedUnixTime = DateTimeExtensions.ToUnixTimeSecond(DateTime.Now),
        };

        return From(data, envelopeMetadata);
    }
}
