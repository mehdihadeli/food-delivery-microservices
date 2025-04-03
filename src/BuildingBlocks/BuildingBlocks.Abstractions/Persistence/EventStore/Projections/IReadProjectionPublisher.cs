using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Messages;

namespace BuildingBlocks.Abstractions.Persistence.EventStore.Projections;

public interface IReadProjectionPublisher
{
    Task PublishAsync(IStreamEventEnvelopeBase streamEvent, CancellationToken cancellationToken = default);

    Task PublishAsync<T>(IStreamEventEnvelope<T> streamEvent, CancellationToken cancellationToken = default)
        where T : class, IDomainEvent;
}
