using BuildingBlocks.Abstractions.Messaging.Context;

namespace BuildingBlocks.Abstractions.Messaging;

public interface IMessageHandler<in TMessage>
    where TMessage : class, IMessage
{
    Task HandleAsync(IConsumeContext<TMessage> messageContext, CancellationToken cancellationToken = default);
}
