using BuildingBlocks.Abstractions.Events;

namespace BuildingBlocks.Abstractions.Messaging;

public interface IMessageHandler<in TMessage>
    where TMessage : class, IMessage
{
    Task HandleAsync(IEventEnvelope<TMessage> eventEnvelope, CancellationToken cancellationToken = default);
}
