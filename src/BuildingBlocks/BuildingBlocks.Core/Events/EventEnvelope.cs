using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Messaging.Extensions;
using BuildingBlocks.Core.Types;
using BuildingBlocks.Core.Types.Extensions;
using Humanizer;
using MassTransit;
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

    public static IEventEnvelope<TMessage> From<TMessage>(TMessage message, IDictionary<string, object?>? headers)
        where TMessage : class, IMessage
    {
        var metaHeaders = headers ?? new Dictionary<string, object?>();

        if (!metaHeaders.ContainsKey(MessageHeaders.MessageId))
        {
            var messageId = message.MessageId;
            metaHeaders.AddMessageId(messageId);
        }

        if (!metaHeaders.ContainsKey(MessageHeaders.CorrelationId))
        {
            metaHeaders.AddCorrelationId(NewId.NextGuid());
        }

        metaHeaders.AddMessageName(message.GetType().Name.Underscore());
        metaHeaders.AddMessageType(TypeMapper.GetTypeName(message.GetType()));
        metaHeaders.AddCreatedUnixTime(DateTimeExtensions.ToUnixTimeSecond(DateTime.Now));

        var envelopeMetadata = new EventEnvelopeMetadata(
            metaHeaders.GetMessageId(),
            metaHeaders.GetCorrelationId(),
            metaHeaders.GetMessageType(),
            metaHeaders.GetMessageName(),
            metaHeaders
        )
        {
            CreatedUnixTime = metaHeaders.GetCreatedUnixTime()
        };

        return new EventEnvelope<TMessage>(message, envelopeMetadata);
    }

    private static IEventEnvelope From(object message, IDictionary<string, object?>? headers)
    {
        var metaHeaders = headers ?? new Dictionary<string, object?>();

        if (!metaHeaders.ContainsKey(MessageHeaders.MessageId))
        {
            //TODO: Using snowflake id here
            var messageId = Guid.NewGuid();
            metaHeaders.AddMessageId(messageId.ToString());
        }

        if (!metaHeaders.ContainsKey(MessageHeaders.CorrelationId))
        {
            metaHeaders.AddCorrelationId(Guid.NewGuid().ToString());
        }

        metaHeaders.AddMessageName(message.GetType().Name.Underscore());
        metaHeaders.AddMessageType(TypeMapper.GetTypeName(message.GetType())); // out of process message should have just type name instead of type full name
        metaHeaders.AddCreatedUnixTime(DateTimeExtensions.ToUnixTimeSecond(DateTime.Now));

        var envelopeMetadata = new EventEnvelopeMetadata(
            metaHeaders.GetMessageId(),
            metaHeaders.GetCorrelationId(),
            metaHeaders.GetMessageType(),
            metaHeaders.GetMessageName(),
            metaHeaders
        )
        {
            CreatedUnixTime = metaHeaders.GetCreatedUnixTime()
        };

        return new EventEnvelope(message, envelopeMetadata);
    }
}
