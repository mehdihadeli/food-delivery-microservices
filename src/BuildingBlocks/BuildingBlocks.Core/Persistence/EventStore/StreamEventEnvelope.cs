using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Persistence.EventStore;

namespace BuildingBlocks.Core.Persistence.EventStore;

public record StreamEventEnvelope<T>(T Data, IStreamEventMetadata Metadata) : IStreamEventEnvelope<T>
    where T : IDomainEvent
{
    object IStreamEventEnvelope.Data => Data;
}

public record StreamEvent(object Data, IStreamEventMetadata Metadata) : IStreamEventEnvelope;

public static class StreamEventFactory
{
    public static IStreamEventEnvelope From(object data, IStreamEventMetadata metadata)
    {
        //TODO: Get rid of reflection!
        var type = typeof(StreamEventEnvelope<>).MakeGenericType(data.GetType());
        return (IStreamEventEnvelope)Activator.CreateInstance(type, data, metadata)!;
    }
}
