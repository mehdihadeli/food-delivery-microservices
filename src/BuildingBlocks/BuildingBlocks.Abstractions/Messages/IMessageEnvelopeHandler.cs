namespace BuildingBlocks.Abstractions.Messages;

public interface IMessageEnvelopeHandler<in TMessage>
    where TMessage : class, IMessage
{
    Task Handle(IMessageEnvelope<TMessage> messageEnvelope, CancellationToken cancellationToken = default);
}
