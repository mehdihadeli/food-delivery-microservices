using System.Reflection;
using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Core.Types;
using BuildingBlocks.Core.Types.Extensions;
using Humanizer;

namespace BuildingBlocks.Core.Events;

internal record EventEnvelope<T>(T Data, IEventEnvelopeMetadata? Metadata) : IEventEnvelope<T>
    where T : notnull
{
    object IEventEnvelope.Data => Data;
}

public static class EventEnvelope
{
    public static IEventEnvelope From(object data, IEventEnvelopeMetadata? metadata)
    {
        var type = typeof(EventEnvelope<>).MakeGenericType(data.GetType());
        return (IEventEnvelope)Activator.CreateInstance(type, data, metadata)!;
    }

    public static IEventEnvelope<TMessage> From<TMessage>(TMessage data, IEventEnvelopeMetadata? metadata)
        where TMessage : IMessage
    {
        return new EventEnvelope<TMessage>(data, metadata);
    }

    public static IEventEnvelope From(object data, Guid correlationId, Guid? cautionId = null)
    {
        var methodInfo = typeof(EventEnvelope).GetMethod(nameof(From), BindingFlags.NonPublic | BindingFlags.Static);
        var genericMethod = methodInfo.MakeGenericMethod(data.GetType());

        return (IEventEnvelope)genericMethod.Invoke(null, new object[] { data, correlationId, cautionId });
    }

    public static IEventEnvelope<TMessage> From<TMessage>(TMessage data, Guid correlationId, Guid? cautionId = null)
        where TMessage : IMessage
    {
        var envelopeMetadata = new EventEnvelopeMetadata(
            data.MessageId,
            correlationId,
            TypeMapper.GetTypeName(data.GetType()),
            data.GetType().Name.Underscore(),
            cautionId
        )
        {
            CreatedUnixTime = DateTime.Now.ToUnixTimeSecond(),
        };

        return From(data, envelopeMetadata);
    }
}
