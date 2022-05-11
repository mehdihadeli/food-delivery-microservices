using BuildingBlocks.Abstractions.CQRS.Event.Internal;
using BuildingBlocks.Abstractions.Messaging;

namespace BuildingBlocks.Core.Messaging.MessagePersistence;

public class NullMessagePersistenceService : IMessagePersistenceService
{
    public Task AddPublishMessageAsync<TMessageEnvelope>(
        TMessageEnvelope messageEnvelope,
        CancellationToken cancellationToken = default)
        where TMessageEnvelope : MessageEnvelope
    {
        return Task.CompletedTask;
    }

    public Task AddReceivedMessageAsync<TMessageEnvelope>(
        TMessageEnvelope messageEnvelope,
        CancellationToken cancellationToken = default)
        where TMessageEnvelope : MessageEnvelope
    {
        return Task.CompletedTask;
    }

    public Task AddInternalMessageAsync<TMessageEnvelope>(
        TMessageEnvelope messageEnvelope,
        CancellationToken cancellationToken = default)
        where TMessageEnvelope : MessageEnvelope
    {
        return Task.CompletedTask;
    }

    public Task AddNotificationAsync(
        IDomainNotificationEvent notification,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task ProcessAsync(
        Guid messageId,
        MessageDeliveryType deliveryType,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task ProcessAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
