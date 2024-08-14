using BuildingBlocks.Abstractions.Events;

namespace BuildingBlocks.Abstractions.Persistence.EventStore.Projections;

public interface IReadProjectionPublisher
{
    Task PublishAsync(IStreamEventEnvelope streamEvent, CancellationToken cancellationToken = default);

    Task PublishAsync<T>(IStreamEventEnvelope<T> streamEvent, CancellationToken cancellationToken = default)
        where T : IDomainEvent;
}
