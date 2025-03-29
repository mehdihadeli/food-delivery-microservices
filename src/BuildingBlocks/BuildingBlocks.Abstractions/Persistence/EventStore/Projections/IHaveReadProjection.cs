using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Messages;

namespace BuildingBlocks.Abstractions.Persistence.EventStore.Projections;

public interface IHaveReadProjection
{
    Task ProjectAsync<T>(IStreamEventEnvelope<T> iStreamEvent, CancellationToken cancellationToken = default)
        where T : class, IDomainEvent;
}
