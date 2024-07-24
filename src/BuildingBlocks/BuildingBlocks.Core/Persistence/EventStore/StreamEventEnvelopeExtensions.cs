using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Persistence.EventStore;
using BuildingBlocks.Core.Reflection;

namespace BuildingBlocks.Core.Persistence.EventStore;

public static class StreamEventEnvelopeExtensions
{
    public static IStreamEventEnvelope ToStreamEvent(this IDomainEvent domainEvent, IStreamEventMetadata? metadata)
    {
        return ReflectionUtilities.CreateGenericType(
            typeof(StreamEventEnvelope<>),
            new[] { domainEvent.GetType() },
            domainEvent,
            metadata
        );
    }
}
