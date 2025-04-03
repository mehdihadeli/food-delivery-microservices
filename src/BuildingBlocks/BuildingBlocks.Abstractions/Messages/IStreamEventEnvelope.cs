using BuildingBlocks.Abstractions.Events;

namespace BuildingBlocks.Abstractions.Messages;

public interface IStreamEventEnvelope<out T> : IStreamEventEnvelopeBase
    where T : class, IDomainEvent
{
    new T Data { get; }
}
