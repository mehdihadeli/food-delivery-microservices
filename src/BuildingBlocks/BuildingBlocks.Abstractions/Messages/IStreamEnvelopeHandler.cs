using BuildingBlocks.Abstractions.Events;

namespace BuildingBlocks.Abstractions.Messages;

public interface IStreamEnvelopeHandler<in TStreamEvent>
    where TStreamEvent : class, IDomainEvent
{
    Task Handle(IStreamEventEnvelope<TStreamEvent> streamEventEnvelope, CancellationToken cancellationToken = default);
}
