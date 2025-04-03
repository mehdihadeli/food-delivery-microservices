using BuildingBlocks.Abstractions.Events;

namespace BuildingBlocks.Abstractions.Messages;

public interface IMessagePublisher
{
    Task Publish<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class, IMessage;

    Task Publish<TMessage>(IMessageEnvelope<TMessage> messageEnvelope, CancellationToken cancellationToken = default)
        where TMessage : class, IMessage;

    Task Publish<TStreamEvent>(
        IStreamEventEnvelope<TStreamEvent> streamEventEnvelope,
        CancellationToken cancellationToken = default
    )
        where TStreamEvent : class, IDomainEvent;
}
