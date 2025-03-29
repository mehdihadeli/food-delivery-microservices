using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Messages;

namespace BuildingBlocks.Core.Messages;

public static class StreamEventEnvelopeFactory
{
    public static IStreamEventEnvelope<T> From<T>(T data, StreamEventMetadata? metadata)
        where T : class, IDomainEvent
    {
        return new StreamEventEnvelope<T>(data, metadata);
    }

    public static IStreamEventEnvelopeBase From(object data, StreamEventMetadata? metadata)
    {
        var methodInfo = typeof(StreamEventEnvelopeFactory)
            .GetMethods()
            .FirstOrDefault(x =>
                string.Equals(x.Name, nameof(From), StringComparison.OrdinalIgnoreCase)
                && x.GetGenericArguments().Length != 0
                && x.GetParameters().Length == 2
            );
        var genericMethod = methodInfo.MakeGenericMethod(data.GetType());

        return (IStreamEventEnvelopeBase)genericMethod.Invoke(null, [data, metadata])!;
    }

    public static IStreamEventEnvelope<T> From<T>(T data, string eventId, ulong streamPosition, ulong logPosition)
        where T : class, IDomainEvent
    {
        var envelopeMetadata = new StreamEventMetadata(eventId, streamPosition, logPosition);

        return From(data, envelopeMetadata);
    }

    public static IStreamEventEnvelopeBase From(object data, string eventId, ulong streamPosition, ulong logPosition)
    {
        var methodInfo = typeof(StreamEventEnvelopeFactory)
            .GetMethods()
            .FirstOrDefault(x =>
                string.Equals(x.Name, nameof(From), StringComparison.OrdinalIgnoreCase)
                && x.GetGenericArguments().Length != 0
                && x.GetParameters().Length == 4
            );
        var genericMethod = methodInfo.MakeGenericMethod(data.GetType());

        return (IStreamEventEnvelopeBase)genericMethod.Invoke(null, [data, eventId, streamPosition, logPosition])!;
    }
}
