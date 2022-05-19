using BuildingBlocks.Abstractions.CQRS.Events.Internal;

namespace BuildingBlocks.Abstractions.Persistence.EventStore.Projections;

public interface IHaveReadProjection
{
    Task ProjectAsync<T>(IStreamEvent<T> streamEvent, CancellationToken cancellationToken = default)
        where T : IDomainEvent;
}
