using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Core.Types;
using BuildingBlocks.Core.Types.Extensions;
using Humanizer;

namespace BuildingBlocks.Core.Events;

// For deserialization of EventEnvelope, EventEnvelopeMetadata should be a class here
public record EventEnvelope<T>(T Message, EventEnvelopeMetadata Metadata) : IEventEnvelope<T>
    where T : IMessage
{
    object IEventEnvelope.Message => Message;
}

public static class EventEnvelope
{
    public static IEventEnvelope From(object data, EventEnvelopeMetadata metadata)
    {
        var type = typeof(EventEnvelope<>).MakeGenericType(data.GetType());
        return (IEventEnvelope)Activator.CreateInstance(type, data, metadata)!;
    }

    public static IEventEnvelope<T> From<T>(T data, EventEnvelopeMetadata metadata)
        where T : IMessage
    {
        return new EventEnvelope<T>(data, metadata);
    }

    public static IEventEnvelope From(
        object data,
        Guid correlationId,
        Guid? cautionId = null,
        IDictionary<string, object?>? headers = null
    )
    {
        var methodInfo = typeof(EventEnvelope)
            .GetMethods()
            .FirstOrDefault(x =>
                x.Name == nameof(From) && x.GetGenericArguments().Length != 0 && x.GetParameters().Length == 4
            );
        var genericMethod = methodInfo.MakeGenericMethod(data.GetType());

        return (IEventEnvelope)genericMethod.Invoke(null, new object[] { data, correlationId, cautionId, headers });
    }

    public static IEventEnvelope<T> From<T>(
        T data,
        Guid correlationId,
        Guid? cautionId = null,
        IDictionary<string, object?>? headers = null
    )
        where T : IMessage
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
            Headers = headers ?? new Dictionary<string, object?>()
        };

        return From(data, envelopeMetadata);
    }
}
