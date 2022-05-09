using BuildingBlocks.Abstractions.CQRS.Event.Internal;
using BuildingBlocks.Abstractions.Persistence.EventStore;
using BuildingBlocks.Core.Utils;

namespace BuildingBlocks.Core.Persistence.EventStore;

public static class StreamEventExtensions
{
    public static IStreamEvent ToStreamEvent(
        this IDomainEvent domainEvent,
        IStreamEventMetadata? metadata)
    {
        return ReflectionUtilities.CreateGenericType(
            typeof(StreamEvent<>),
            new[] { domainEvent.GetType() },
            domainEvent,
            metadata);
    }
}
