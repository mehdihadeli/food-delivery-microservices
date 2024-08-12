using System.Reflection;
using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Persistence.EventStore;

namespace BuildingBlocks.Core.Persistence.EventStore;

public record StreamEventEnvelope<T>(T Data, IStreamEventMetadata? Metadata) : IStreamEventEnvelope<T>
    where T : IDomainEvent
{
    object IStreamEventEnvelope.Data => Data;
}

public static class StreamEventEnvelope
{
    public static IStreamEventEnvelope From(object data, IStreamEventMetadata? metadata)
    {
        var type = typeof(StreamEventEnvelope<>).MakeGenericType(data.GetType());
        return (IStreamEventEnvelope)Activator.CreateInstance(type, data, metadata)!;
    }

    public static IStreamEventEnvelope<TMessage> From<TMessage>(TMessage data, IStreamEventMetadata? metadata)
        where TMessage : IDomainEvent
    {
        return new StreamEventEnvelope<TMessage>(data, metadata);
    }

    public static IStreamEventEnvelope From(object data, string eventId, ulong streamPosition, ulong logPosition)
    {
        var methodInfo = typeof(StreamEventEnvelope).GetMethod(
            nameof(From),
            BindingFlags.NonPublic | BindingFlags.Static
        );
        var genericMethod = methodInfo!.MakeGenericMethod(data.GetType());

        return (IStreamEventEnvelope)
            genericMethod.Invoke(null, new object[] { data, eventId, streamPosition, logPosition });
    }

    public static IStreamEventEnvelope<T> From<T>(T data, string eventId, ulong streamPosition, ulong logPosition)
        where T : IDomainEvent
    {
        var envelopeMetadata = new StreamEventMetadata(eventId, streamPosition, logPosition);

        return From(data, envelopeMetadata);
    }
}
