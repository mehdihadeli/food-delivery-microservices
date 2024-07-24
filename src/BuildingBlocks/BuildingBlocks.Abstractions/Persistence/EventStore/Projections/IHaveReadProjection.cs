using BuildingBlocks.Abstractions.Domain.Events.Internal;

namespace BuildingBlocks.Abstractions.Persistence.EventStore.Projections;

public interface IHaveReadProjection
{
    Task ProjectAsync<T>(IStreamEventEnvelope<T> streamEvent, CancellationToken cancellationToken = default)
        where T : IDomainEvent;
}
