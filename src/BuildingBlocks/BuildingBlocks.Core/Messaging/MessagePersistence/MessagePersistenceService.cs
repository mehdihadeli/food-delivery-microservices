using System.Linq.Expressions;
using System.Reflection;
using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Events.Internal;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Abstractions.Serialization;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Types;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Core.Messaging.MessagePersistence;

public class MessagePersistenceService(
    ILogger<MessagePersistenceService> logger,
    IMessagePersistenceRepository messagePersistenceRepository,
    IMessageSerializer messageSerializer,
    IMediator mediator,
    IBusDirectPublisher busDirectPublisher,
    ISerializer serializer
) : IMessagePersistenceService
{
    public Task<IReadOnlyList<StoreMessage>> GetByFilterAsync(
        Expression<Func<StoreMessage, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    )
    {
        return messagePersistenceRepository.GetByFilterAsync(predicate ?? (_ => true), cancellationToken);
    }

    public async Task AddPublishMessageAsync<TMessage>(
        IEventEnvelope<TMessage> eventEnvelope,
        CancellationToken cancellationToken = default
    )
        where TMessage : IMessage
    {
        await AddMessageCore(eventEnvelope, MessageDeliveryType.Outbox, cancellationToken);
    }

    public async Task AddReceivedMessageAsync<TMessage>(
        IEventEnvelope<TMessage> eventEnvelope,
        CancellationToken cancellationToken = default
    )
        where TMessage : IMessage
    {
        await AddMessageCore(eventEnvelope, MessageDeliveryType.Inbox, cancellationToken);
    }

    public async Task AddInternalMessageAsync<TInternalCommand>(
        TInternalCommand internalCommand,
        CancellationToken cancellationToken = default
    )
        where TInternalCommand : IInternalCommand
    {
        await messagePersistenceRepository.AddAsync(
            new StoreMessage(
                internalCommand.InternalCommandId,
                TypeMapper.GetFullTypeName(internalCommand.GetType()), // same process so we use full type name
                serializer.Serialize(internalCommand),
                MessageDeliveryType.Internal
            ),
            cancellationToken
        );
    }

    public async Task AddNotificationAsync<TDomainNotification>(
        TDomainNotification notification,
        CancellationToken cancellationToken = default
    )
        where TDomainNotification : IDomainNotificationEvent
    {
        await messagePersistenceRepository.AddAsync(
            new StoreMessage(
                notification.EventId,
                TypeMapper.GetFullTypeName(notification.GetType()), // same process so we use full type name
                serializer.Serialize(notification),
                MessageDeliveryType.Internal
            ),
            cancellationToken
        );
    }

    private async Task AddMessageCore(
        IEventEnvelope eventEnvelope,
        MessageDeliveryType deliveryType,
        CancellationToken cancellationToken = default
    )
    {
        eventEnvelope.Message.NotBeNull();

        var id = eventEnvelope.Message is IMessage im ? im.MessageId : Guid.NewGuid();

        await messagePersistenceRepository.AddAsync(
            new StoreMessage(
                id,
                TypeMapper.GetFullTypeName(eventEnvelope.Message.GetType()), // because each service has its own persistence and inbox and outbox processor will run in the same process we can use full type name
                messageSerializer.Serialize(eventEnvelope),
                deliveryType
            ),
            cancellationToken
        );

        logger.LogInformation(
            "Message with id: {MessageID} and delivery type: {DeliveryType} saved in persistence message store",
            id,
            deliveryType.ToString()
        );
    }

    public async Task ProcessAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        var message = await messagePersistenceRepository.GetByIdAsync(messageId, cancellationToken);

        if (message is null)
            return;

        switch (message.DeliveryType)
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

        await messagePersistenceRepository.ChangeStateAsync(message.Id, MessageStatus.Processed, cancellationToken);
    }

    public async Task ProcessAllAsync(CancellationToken cancellationToken = default)
    {
        var messages = await messagePersistenceRepository.GetByFilterAsync(
            x => x.MessageStatus != MessageStatus.Processed,
            cancellationToken
        );

        foreach (var message in messages)
        {
            await ProcessAsync(message.Id, cancellationToken);
        }
    }

    private async Task ProcessOutbox(StoreMessage storeMessage, CancellationToken cancellationToken)
    {
        var messageType = TypeMapper.GetType(storeMessage.DataType);
        var eventEnvelope = messageSerializer.Deserialize(storeMessage.Data, messageType);

        if (eventEnvelope is null)
            return;

        // eventEnvelope.Metadata.Headers.TryGetValue(MessageHeaders.ExchangeOrTopic, out var exchange);
        // eventEnvelope.Metadata.Headers.TryGetValue(MessageHeaders.Queue, out var queue);

        // we should pass an object type message or explicit our message type, not cast to IMessage (data is IMessage integrationEvent) because masstransit doesn't work with IMessage cast.
        await busDirectPublisher.PublishAsync(eventEnvelope, cancellationToken);

        logger.LogInformation(
            "Message with id: {MessageId} and delivery type: {DeliveryType} processed from the persistence message store",
            storeMessage.Id,
            storeMessage.DeliveryType
        );
    }

    private async Task ProcessInternal(StoreMessage storeMessage, CancellationToken cancellationToken)
    {
        var messageType = TypeMapper.GetType(storeMessage.DataType);
        var internalMessage = serializer.Deserialize(storeMessage.Data, messageType);

        if (internalMessage is null)
            return;

        if (internalMessage is IDomainNotificationEvent domainNotificationEvent)
        {
            await mediator.Publish(domainNotificationEvent, cancellationToken);

            logger.LogInformation(
                "Domain-Notification with id: {EventID} and delivery type: {DeliveryType} processed from the persistence message store",
                storeMessage.Id,
                storeMessage.DeliveryType
            );
        }

        if (internalMessage is IInternalCommand internalCommand)
        {
            await mediator.Send(internalCommand, cancellationToken);

            logger.LogInformation(
                "InternalCommand with id: {EventID} and delivery type: {DeliveryType} processed from the persistence message store",
                storeMessage.Id,
                storeMessage.DeliveryType
            );
        }
    }

    private Task ProcessInbox(StoreMessage storeMessage, CancellationToken cancellationToken)
    {
        var messageType = TypeMapper.GetType(storeMessage.DataType);
        var messageEnvelope = messageSerializer.Deserialize(storeMessage.Data, messageType);

        return Task.CompletedTask;
    }
}
