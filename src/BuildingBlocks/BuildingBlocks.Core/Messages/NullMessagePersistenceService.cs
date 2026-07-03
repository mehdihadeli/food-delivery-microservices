using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Abstractions.Messages.MessagePersistence;

namespace BuildingBlocks.Core.Messages;

public class NullMessagePersistenceService : IMessagePersistenceService
{
    public Task AddPublishMessageAsync(
        IMessageEnvelopeBase messageEnvelope,
        CancellationToken cancellationToken = default
    )
    {
        return Task.CompletedTask;
    }

    public Task AddReceivedMessageAsync<TMessage>(
        IMessageEnvelopeBase messageEnvelope,
        Func<IMessageEnvelopeBase, Task> dispatchAction,
        CancellationToken cancellationToken = default
    )
    {
        return dispatchAction(messageEnvelope);
    }

    public Task AddInternalMessageAsync<TInternalCommand>(
        TInternalCommand internalCommand,
        CancellationToken cancellationToken = default
    )
        where TInternalCommand : IInternalCommand
    {
        return Task.CompletedTask;
    }

    public Task AddNotificationAsync<TDomainNotification>(
        TDomainNotification notification,
        CancellationToken cancellationToken = default
    )
        where TDomainNotification : IDomainNotificationEvent<IDomainEvent>
    {
        return Task.CompletedTask;
    }
}
