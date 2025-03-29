namespace BuildingBlocks.Abstractions.Messages;

public interface IMessageHandler<in TMessage>
    where TMessage : class, IMessage
{
    Task Handle(TMessage message, CancellationToken cancellationToken = default);
}
