using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Abstractions.CQRS.Event.Internal;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Abstractions.Serialization;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Types;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Core.Messaging.MessagePersistence.InMemory;

public class InMemoryMessagePersistenceService : IMessagePersistenceService
{
    private readonly ILogger<InMemoryMessagePersistenceService> _logger;
    private readonly InMemoryMessagePersistenceRepository _inMemoryMessagePersistenceRepository;
    private readonly IMessageSerializer _messageSerializer;
    private readonly IMediator _mediator;
    private readonly IBus _bus;
    private readonly ISerializer _serializer;

    public InMemoryMessagePersistenceService(
        ILogger<InMemoryMessagePersistenceService> logger,
        InMemoryMessagePersistenceRepository inMemoryMessagePersistenceRepository,
        IMessageSerializer messageSerializer,
        IMediator mediator,
        IBus bus,
        ISerializer serializer)
    {
        _logger = logger;
        _inMemoryMessagePersistenceRepository = inMemoryMessagePersistenceRepository;
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

    public async Task AddInternalMessageAsync<TCommand>(
        TCommand internalCommand,
        CancellationToken cancellationToken = default)
        where TCommand : class, IInternalCommand
    {
        await AddMessageCore(new MessageEnvelope(internalCommand), MessageDeliveryType.Internal, cancellationToken);
    }

    public async Task AddNotificationAsync(
        IDomainNotificationEvent notification,
        CancellationToken cancellationToken = default)
    {
        await _inMemoryMessagePersistenceRepository.AddAsync(
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
        Guard.Against.Null(messageEnvelope.Message, nameof(messageEnvelope.Message));

        Guid id;
        if (messageEnvelope.Message is IMessage im)
        {
            id = im.MessageId;
        }
        else if (messageEnvelope.Message is IInternalCommand command)
        {
            id = command.Id;
        }
        else
        {
            id = Guid.NewGuid();
        }

        await _inMemoryMessagePersistenceRepository.AddAsync(
            new StoreMessage(
                id,
                TypeMapper.GetTypeName(messageEnvelope.Message.GetType()),
                _messageSerializer.Serialize(messageEnvelope),
                deliveryType),
            cancellationToken);

        _logger.LogInformation(
            "Message with id: {MessageID} and delivery type: {DeliveryType} saved in persistence message store.",
            id,
            deliveryType.ToString());
    }

    public async Task ProcessAsync(
        Guid messageId,
        MessageDeliveryType deliveryType,
        CancellationToken cancellationToken = default)
    {
        var message = (await _inMemoryMessagePersistenceRepository
                .GetByFilterAsync(x => x.Id == messageId && x.DeliveryType == deliveryType, cancellationToken))
            .FirstOrDefault();

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
    }

    public async Task ProcessAllAsync(CancellationToken cancellationToken = default)
    {
        var messages = await _inMemoryMessagePersistenceRepository
            .GetByFilterAsync(x => x.MessageStatus != MessageStatus.Processed, cancellationToken);

        foreach (var message in messages)
        {
            await ProcessAsync(message.Id, message.DeliveryType, cancellationToken);
        }
    }

    private async Task ProcessOutbox(StoreMessage message, CancellationToken cancellationToken)
    {
        MessageEnvelope? messageEnvelope = _messageSerializer.Deserialize<MessageEnvelope>(message.Data, true);

        if (messageEnvelope is null || messageEnvelope.Message is null)
            return;

        var data = _messageSerializer.Deserialize(
            messageEnvelope.Message.ToString()!,
            TypeMapper.GetType(message.DataType));

        if (data is IMessage)
        {
            // we should pass a object type message or explicit our message type, not cast to IMessage (data is IMessage integrationEvent) because masstransit doesn't work with IMessage cast.
            await _bus.PublishAsync(
                data,
                messageEnvelope.Headers,
                cancellationToken);

            _logger.LogInformation(
                "Message with id: {MessageId} and delivery type: {DeliveryType} processed from the persistence message store.",
                message.Id,
                message.DeliveryType);
        }
    }

    private async Task ProcessInternal(StoreMessage message, CancellationToken cancellationToken)
    {
        MessageEnvelope? messageEnvelope = _messageSerializer.Deserialize<MessageEnvelope>(message.Data, true);

        if (messageEnvelope is null || messageEnvelope.Message is null)
            return;

        var data = _messageSerializer.Deserialize(
            messageEnvelope.Message.ToString()!,
            TypeMapper.GetType(message.DataType));

        if (data is IDomainNotificationEvent domainNotificationEvent)
        {
            await _mediator.Publish(domainNotificationEvent, cancellationToken);

            _logger.LogInformation(
                "Domain-Notification with id: {EventID} and delivery type: {DeliveryType} processed from the persistence message store.",
                message.Id,
                message.DeliveryType);
        }

        if (data is IInternalCommand internalCommand)
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
