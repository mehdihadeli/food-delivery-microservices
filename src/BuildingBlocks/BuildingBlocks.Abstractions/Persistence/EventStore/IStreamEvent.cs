using BuildingBlocks.Abstractions.Domain.Events.Internal;

namespace BuildingBlocks.Abstractions.Persistence.EventStore;

public interface IStreamEvent
{
    object Data { get; }
    IStreamEventMetadata Metadata { get; init; }
}

public interface IStreamEvent<out T> : IStreamEvent
    where T : IDomainEvent
{
    new T Data { get; }
}
