using System.Linq.Expressions;
using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Events.Internal;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Abstractions.Serialization;
using BuildingBlocks.Core.Events;
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
    IExternalEventBus bus,
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

    public async Task AddPublishMessageAsync<TEventEnvelope>(
        TEventEnvelope eventEnvelope,
        CancellationToken cancellationToken = default
    )
        where TEventEnvelope : IEventEnvelope
    {
        await AddMessageCore(eventEnvelope, MessageDeliveryType.Outbox, cancellationToken);
    }

    public async Task AddPublishMessageAsync<TEventEnvelope, TMessage>(
        TEventEnvelope eventEnvelope,
        CancellationToken cancellationToken = default
    )
        where TEventEnvelope : IEventEnvelope<TMessage>
        where TMessage : IMessage
    {
        await AddMessageCore(eventEnvelope, MessageDeliveryType.Outbox, cancellationToken);
    }

    public async Task AddReceivedMessageAsync<TEventEnvelope>(
        TEventEnvelope messageEnvelope,
        CancellationToken cancellationToken = default
    )
        where TEventEnvelope : IEventEnvelope
    {
        await AddMessageCore(messageEnvelope, MessageDeliveryType.Inbox, cancellationToken);
    }

    public async Task AddInternalMessageAsync<TCommand>(
        TCommand internalCommand,
        CancellationToken cancellationToken = default
    )
        where TCommand : class, IInternalCommand
    {
        await AddMessageCore(
            EventEnvelope.From(internalCommand, metadata: null),
            MessageDeliveryType.Internal,
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
        eventEnvelope.Data.NotBeNull();

        Guid id;
        if (eventEnvelope.Data is IMessage im)
        {
            id = im.MessageId;
        }
        else if (eventEnvelope.Data is IInternalCommand command)
        {
            id = command.InternalCommandId;
        }
        else
        {
            id = Guid.NewGuid();
        }

        await messagePersistenceRepository.AddAsync(
            new StoreMessage(
                id,
                TypeMapper.GetFullTypeName(eventEnvelope.Data.GetType()), // because each service has its own persistence and same process (outbox,inbox), full name message type but in microservices we should just use type name for message
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

        var messageEnvelope = messageSerializer.Deserialize(storeMessage.Data, messageType);

        if (messageEnvelope is null)
            return;

        // we should pass an object type message or explicit our message type, not cast to IMessage (data is IMessage integrationEvent) because masstransit doesn't work with IMessage cast.
        await bus.PublishAsync(messageEnvelope, cancellationToken);

        logger.LogInformation(
            "Message with id: {MessageId} and delivery type: {DeliveryType} processed from the persistence message store",
            storeMessage.Id,
            storeMessage.DeliveryType
        );
    }

    private async Task ProcessInternal(StoreMessage storeMessage, CancellationToken cancellationToken)
    {
        var messageEnvelope = messageSerializer.Deserialize(storeMessage.Data);

        if (messageEnvelope is null)
            return;

        if (messageEnvelope.Data is IDomainNotificationEvent domainNotificationEvent)
        {
            await mediator.Publish(domainNotificationEvent, cancellationToken);

            logger.LogInformation(
                "Domain-Notification with id: {EventID} and delivery type: {DeliveryType} processed from the persistence message store",
                storeMessage.Id,
                storeMessage.DeliveryType
            );
        }

        if (messageEnvelope.Data is IInternalCommand internalCommand)
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
        return Task.CompletedTask;
    }
}
