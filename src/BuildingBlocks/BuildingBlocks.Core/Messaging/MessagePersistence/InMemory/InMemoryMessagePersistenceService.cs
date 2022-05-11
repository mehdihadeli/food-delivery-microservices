using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Abstractions.CQRS.Event.Internal;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Serialization;
using BuildingBlocks.Core.Messaging.Extensions;
using BuildingBlocks.Core.Types;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Core.Messaging.MessagePersistence.InMemory;

public class InMemoryMessagePersistenceService : IMessagePersistenceService
{
    private readonly ILogger<InMemoryMessagePersistenceService> _logger;
    private readonly InMemoryMessagePersistenceContext _inMemoryMessagePersistenceContext;
    private readonly IMessageSerializer _messageSerializer;
    private readonly IMediator _mediator;
    private readonly IBus _bus;
    private readonly ISerializer _serializer;

    public InMemoryMessagePersistenceService(
        ILogger<InMemoryMessagePersistenceService> logger,
        InMemoryMessagePersistenceContext inMemoryMessagePersistenceContext,
        IMessageSerializer messageSerializer,
        IMediator mediator,
        IBus bus,
        ISerializer serializer)
    {
        _logger = logger;
        _inMemoryMessagePersistenceContext = inMemoryMessagePersistenceContext;
        _messageSerializer = messageSerializer;
        _mediator = mediator;
        _bus = bus;
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

        _logger.LogInformation(
            "Message with id: {MessageID} and delivery type: {DeliveryType} saved in persistence message store.",
            messageEnvelope.GetMessageId(),
            deliveryType.ToString());
    }

    public async Task ProcessAsync(
        Guid messageId,
        MessageDeliveryType deliveryType,
        CancellationToken cancellationToken = default)
    {
        var message = await _inMemoryMessagePersistenceContext.StoreMessages
            .FirstOrDefaultAsync(x => x.Id == messageId && x.DeliveryType == deliveryType, cancellationToken);

        if (message is null)
            return;

        message.ChangeState(MessageStatus.Processed);

        switch (deliveryType)
        {
            case MessageDeliveryType.Inbox:
                await ProcessInbox(message, cancellationToken);
                break;
            case MessageDeliveryType.Internal:
                await ProcessInternal(message, cancellationToken);
                break;
            case MessageDeliveryType.Outbox:
                await ProcessOutbox(message, cancellationToken);
                break;
        }

        await _inMemoryMessagePersistenceContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ProcessAllAsync(CancellationToken cancellationToken = default)
    {
        var messages = _inMemoryMessagePersistenceContext.StoreMessages
            .Where(x => x.MessageStatus != MessageStatus.Processed);

        foreach (var message in messages)
        {
            await ProcessAsync(message.Id, message.DeliveryType, cancellationToken);
        }
    }

    private async Task ProcessOutbox(StoreMessage message, CancellationToken cancellationToken)
    {
        MessageEnvelope? messageEnvelope =
            _messageSerializer.Deserialize(message.Data, message.DataType) as MessageEnvelope;

        if (messageEnvelope is null)
            return;

        if (messageEnvelope.Message is IMessage integrationEvent)
        {
            await _bus.PublishAsync(integrationEvent, messageEnvelope.Headers, cancellationToken);

            _logger.LogInformation(
                "Message with id: {MessageId} and delivery type: {DeliveryType} processed from the persistence message store.",
                message.Id,
                message.DeliveryType);
        }
    }

    private async Task ProcessInternal(StoreMessage message, CancellationToken cancellationToken)
    {
        MessageEnvelope? messageEnvelope =
            _messageSerializer.Deserialize(message.Data, message.DataType) as MessageEnvelope;

        if (messageEnvelope is null)
            return;

        if (messageEnvelope.Message is IDomainNotificationEvent domainNotificationEvent)
        {
            await _mediator.Publish(domainNotificationEvent, cancellationToken);

            _logger.LogInformation(
                "Domain-Notification with id: {EventID} and delivery type: {DeliveryType} processed from the persistence message store.",
                message.Id,
                message.DeliveryType);
        }

        if (messageEnvelope.Message is IInternalCommand internalCommand)
        {
            await _mediator.Send(internalCommand, cancellationToken);

            _logger.LogInformation(
                "InternalCommand with id: {EventID} and delivery type: {DeliveryType} processed from the persistence message store.",
                message.Id,
                message.DeliveryType);
        }
    }

    private Task ProcessInbox(StoreMessage message, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
