using BuildingBlocks.Abstractions.CQRS.Event.Internal;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Serialization;
using BuildingBlocks.Core.Messaging.Extensions;
using BuildingBlocks.Core.Types;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Core.Messaging.MessagePersistence.InMemory;

public class InMemoryMessagePersistenceService : IMessagePersistenceService
{
    private readonly InMemoryMessagePersistenceContext _inMemoryMessagePersistenceContext;
    private readonly IMessageSerializer _messageSerializer;
    private readonly ISerializer _serializer;

    public InMemoryMessagePersistenceService(
        InMemoryMessagePersistenceContext inMemoryMessagePersistenceContext,
        IMessageSerializer messageSerializer,
        ISerializer serializer)
    {
        _inMemoryMessagePersistenceContext = inMemoryMessagePersistenceContext;
        _messageSerializer = messageSerializer;
        _serializer = serializer;
    }

    public async Task AddPublishMessageAsync<TMessageEnvelope>(
        TMessageEnvelope messageEnvelope,
        CancellationToken cancellationToken = default)
        where TMessageEnvelope : MessageEnvelope
    {
        await AddMessageCore(messageEnvelope, MessageDeliveryType.Outbox, cancellationToken);
    }

    public async Task AddReceivedMessageAsync<TMessageEnvelope>(
        TMessageEnvelope messageEnvelope,
        CancellationToken cancellationToken = default)
        where TMessageEnvelope : MessageEnvelope
    {
        await AddMessageCore(messageEnvelope, MessageDeliveryType.Inbox, cancellationToken);
    }

    public async Task AddInternalMessageAsync<TMessageEnvelope>(
        TMessageEnvelope messageEnvelope,
        CancellationToken cancellationToken = default)
        where TMessageEnvelope : MessageEnvelope
    {
        await AddMessageCore(messageEnvelope, MessageDeliveryType.Internal, cancellationToken);
    }

    public async Task AddNotificationAsync(
        IDomainNotificationEvent notification,
        CancellationToken cancellationToken = default)
    {
        await _inMemoryMessagePersistenceContext.StoreMessages.AddAsync(
            new StoreMessage(
                notification.EventId,
                TypeMapper.GetTypeName(notification.GetType()),
                _serializer.Serialize(notification),
                MessageDeliveryType.Internal),
            cancellationToken);
    }

    private async Task AddMessageCore(
        MessageEnvelope messageEnvelope,
        MessageDeliveryType deliveryType,
        CancellationToken cancellationToken = default)
    {
        await _inMemoryMessagePersistenceContext.StoreMessages.AddAsync(
            new StoreMessage(
                messageEnvelope.GetMessageId(),
                TypeMapper.GetTypeName(messageEnvelope.GetType()),
                _messageSerializer.Serialize(messageEnvelope),
                deliveryType),
            cancellationToken);
    }

    public async Task ProcessAsync(
        Guid messageId,
        MessageDeliveryType deliveryType,
        CancellationToken cancellationToken = default)
    {
        var message = await _inMemoryMessagePersistenceContext.StoreMessages
            .FirstOrDefaultAsync(x => x.Id == messageId && x.DeliveryType == deliveryType, cancellationToken);

        message?.ChangeState(MessageStatus.Processed);
    }

    public async Task ProcessAllAsync(
        MessageDeliveryType? deliveryType = null,
        CancellationToken cancellationToken = default)
    {
        var messages = _inMemoryMessagePersistenceContext.StoreMessages
            .Where(x => (deliveryType == null || x.DeliveryType == deliveryType) &&
                        x.MessageStatus != MessageStatus.Processed);

        foreach (var message in messages)
        {
            await ProcessAsync(message.Id, message.DeliveryType, cancellationToken);
        }
    }
}
