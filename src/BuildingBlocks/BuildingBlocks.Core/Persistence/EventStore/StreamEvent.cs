using BuildingBlocks.Abstractions.Domain.Events.Internal;
using BuildingBlocks.Abstractions.Persistence.EventStore;

namespace BuildingBlocks.Core.Persistence.EventStore;

public record StreamEvent<T>(T Data, IStreamEventMetadata Metadata) : IStreamEvent<T>
    where T : IDomainEvent
{
    object IStreamEvent.Data => Data;
}

public record StreamEvent(object Data, IStreamEventMetadata Metadata) : IStreamEvent;

public static class StreamEventFactory
{
    public static IStreamEvent From(object data, IStreamEventMetadata metadata)
    {
        //TODO: Get rid of reflection!
        var type = typeof(StreamEvent<>).MakeGenericType(data.GetType());
        return (IStreamEvent)Activator.CreateInstance(type, data, metadata)!;
    }
}
