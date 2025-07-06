using System.Linq.Expressions;
using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Abstractions.Messages.MessagePersistence;
using BuildingBlocks.Abstractions.Serialization;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Types;
using Medallion.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Core.Messages.MessagePersistence;

public class MessagePersistenceService(
    ILogger<MessagePersistenceService> logger,
    IMessagePersistenceRepository messagePersistenceRepository,
    IMessageSerializer messageSerializer,
    IInternalEventBus internalEventBus,
    IBusDirectPublisher busDirectPublisher,
    ICommandBus commandBus,
    IServiceProvider serviceProvider,
    ISerializer serializer,
    IOptions<MessagePersistenceOptions> options
) : IMessagePersistenceService
{
    public Task<IReadOnlyList<PersistMessage>> GetByFilterAsync(
        Expression<Func<PersistMessage, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    )
    {
        return messagePersistenceRepository.GetByFilterAsync(predicate ?? (_ => true), cancellationToken);
    }

    public Task<IReadOnlyList<PersistMessage>> GetByFilterAsync(
        MessageStatus? status = null,
        MessageDeliveryType? deliveryType = null,
        string? dataType = null,
        CancellationToken cancellationToken = default
    )
    {
        return messagePersistenceRepository.GetByFilterAsync(
            x =>
                (x.MessageStatus == status || status == null)
                && (x.DeliveryType == deliveryType || deliveryType == null)
                && (x.DataType == dataType || dataType == null),
            cancellationToken
        );
    }

    public async Task AddPublishMessageAsync(
        IMessageEnvelopeBase messageEnvelope,
        CancellationToken cancellationToken = default
    )
    {
        messageEnvelope.NotBeNull();
        await AddMessageCore(messageEnvelope, MessageDeliveryType.Outbox, MessageStatus.Stored, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task AddReceivedMessageAsync<TMessage>(
        IMessageEnvelopeBase messageEnvelope,
        Func<IMessageEnvelopeBase, Task> dispatchAction,
        CancellationToken cancellationToken = default
    )
    {
        messageEnvelope.NotBeNull();
        dispatchAction.NotBeNull();
        var messageId = messageEnvelope.Metadata.MessageId;

        // Check if the message already exists in the inbox
        var existingMessages = await messagePersistenceRepository
            .GetByFilterAsync(
                x => x.MessageId == messageId && x.DeliveryType == MessageDeliveryType.Inbox,
                cancellationToken
            )
            .ConfigureAwait(false);

        if (existingMessages.Any())
        {
            logger.LogInformation("Message with ID {MessageId} already exists in the inbox. Skipping.", messageId);
            return;
        }

        // message broker deliver message just to one of consumer based on round-robin algorithm. if consumer fails broker deliver the message to another consumer
        // TODO: executing action and saving entry to inbox should be in the same transaction. because it is possible before the message is saved to the Inbox table the process crashes, broker redelivers the message and consumer will re-run the logic again (duplicate)
        await dispatchAction.Invoke(messageEnvelope).ConfigureAwait(false);
        await AddMessageCore(messageEnvelope, MessageDeliveryType.Inbox, MessageStatus.Delivered, cancellationToken)
            .ConfigureAwait(false);
        logger.LogInformation("Message with id {MessageId} processed from the inbox.", messageId);
    }

    public async Task AddInternalMessageAsync<TInternalCommand>(
        TInternalCommand internalCommand,
        CancellationToken cancellationToken = default
    )
        where TInternalCommand : IInternalCommand
    {
        var id = Guid.NewGuid();
        await messagePersistenceRepository
            .AddAsync(
                new PersistMessage(
                    id,
                    internalCommand.InternalCommandId,
                    // because each service has its own persistence and inbox and outbox processor will run in the same process we can use full type name
                    TypeMapper.AddFullTypeName(internalCommand.GetType()),
                    serializer.Serialize(internalCommand),
                    MessageDeliveryType.Internal,
                    MessageStatus.Stored
                ),
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public async Task AddNotificationAsync<TDomainNotification>(
        TDomainNotification notification,
        CancellationToken cancellationToken = default
    )
        where TDomainNotification : IDomainNotificationEvent<IDomainEvent>
    {
        var id = Guid.NewGuid();
        await messagePersistenceRepository
            .AddAsync(
                new PersistMessage(
                    id,
                    notification.EventId,
                    // because each service has its own persistence and inbox and outbox processor will run in the same process we can use full type name
                    TypeMapper.AddFullTypeName(notification.GetType()),
                    serializer.Serialize(notification),
                    MessageDeliveryType.Internal,
                    MessageStatus.Stored
                ),
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    private async Task AddMessageCore(
        IMessageEnvelopeBase messageEnvelope,
        MessageDeliveryType deliveryType,
        MessageStatus messageStatus = MessageStatus.Stored,
        CancellationToken cancellationToken = default
    )
    {
        messageEnvelope.Message.NotBeNull();

        var id = Guid.NewGuid();

        await messagePersistenceRepository
            .AddAsync(
                new PersistMessage(
                    id,
                    messageEnvelope.Metadata.MessageId,
                    // because each service has its own persistence and inbox and outbox processor will run in the same process we can use full type name
                    TypeMapper.AddFullTypeName(messageEnvelope.Message.GetType()),
                    messageSerializer.Serialize(messageEnvelope),
                    deliveryType,
                    messageStatus: messageStatus
                ),
                cancellationToken
            )
            .ConfigureAwait(false);

        logger.LogInformation(
            "Message with id: {MessageID} and delivery type: {DeliveryType} saved in persistence message store",
            id,
            deliveryType.ToString()
        );
    }

    public async Task ProcessAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        var message = await messagePersistenceRepository
            .GetByIdAsync(messageId, cancellationToken)
            .ConfigureAwait(false);

        if (message is null)
        {
            return;
        }

        switch (message.DeliveryType)
        {
            case MessageDeliveryType.Internal:
                await ProcessInternal(message, cancellationToken).ConfigureAwait(false);
                break;
            case MessageDeliveryType.Outbox:
                await ProcessOutbox(message, cancellationToken).ConfigureAwait(false);
                break;
        }
    }

    public async Task ProcessAllAsync(CancellationToken cancellationToken = default)
    {
        var messages = await messagePersistenceRepository
            .GetByFilterAsync(x => x.MessageStatus != MessageStatus.Delivered, cancellationToken)
            .ConfigureAwait(false);

        foreach (var message in messages)
        {
            await ProcessAsync(message.Id, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task MarkAsDeliveredAsync(Guid messageId, CancellationToken cancellationToken)
    {
        await messagePersistenceRepository
            .ChangeStateAsync(messageId, MessageStatus.Delivered, cancellationToken)
            .ConfigureAwait(false);
        logger.LogInformation("Message with ID {MessageId} marked as delivered.", messageId);
    }

    private async Task ProcessOutbox(PersistMessage persistMessage, CancellationToken cancellationToken)
    {
        var messageId = persistMessage.MessageId;

        // multiple instances can process outbox message in outbox table in the same time, for publishing message based on outbox message on the database. we can accept this becuase we have deduplication in the inbox
        if (options.Value.UseDistributedLock)
        {
            // In a microservices environment with multiple instances of a service running, we need to ensure that only one instance executes a scheduled job, we can handle it with distributed lock
            var lockKey = $"message:{messageId}";

            // In a microservices environment with multiple instances of a service running
            // Acquire a distributed lock with a timeout
            var distributedLockProvider = serviceProvider.GetRequiredService<IDistributedLockProvider>();

            var distributedLock = await distributedLockProvider
                .TryAcquireLockAsync(lockKey, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (distributedLock == null)
            {
                logger.LogInformation(
                    "Failed to acquire lock for message {MessageId}. Another consumer is processing it.",
                    messageId
                );
                return; // Skip processing if the lock cannot be acquired
            }
        }

        var messageType = TypeMapper.GetType(persistMessage.DataType);

        // our data in message persistence is IMessageEnvelope
        var eventEnvelope = messageSerializer.Deserialize(persistMessage.Data, messageType);

        if (eventEnvelope is null)
        {
            return;
        }

        // we should pass an object type message or explicit our message type, not cast to IMessage (data is IMessage integrationEvent) because masstransit doesn't work with IMessage cast.
        await busDirectPublisher.PublishAsync(eventEnvelope, cancellationToken).ConfigureAwait(false);

        await MarkAsDeliveredAsync(persistMessage.MessageId, cancellationToken).ConfigureAwait(false);

        logger.LogInformation(
            "Message with id: {MessageId} and delivery type: {DeliveryType} processed from the persistence message store",
            persistMessage.Id,
            persistMessage.DeliveryType
        );
    }

    private async Task ProcessInternal(PersistMessage persistMessage, CancellationToken cancellationToken)
    {
        var messageType = TypeMapper.GetType(persistMessage.DataType);
        var internalMessage = serializer.Deserialize(persistMessage.Data, messageType);

        if (internalMessage is null)
        {
            return;
        }

        if (internalMessage is IDomainNotificationEvent<IDomainEvent> domainNotificationEvent)
        {
            await internalEventBus.Publish(domainNotificationEvent, cancellationToken).ConfigureAwait(false);

            logger.LogInformation(
                "Domain-Notification with id: {EventID} and delivery type: {DeliveryType} processed from the persistence message store",
                persistMessage.Id,
                persistMessage.DeliveryType
            );
        }

        if (internalMessage is IInternalCommand internalCommand)
        {
            await commandBus.SendAsync(internalCommand, cancellationToken).ConfigureAwait(false);

            logger.LogInformation(
                "InternalCommand with id: {EventID} and delivery type: {DeliveryType} processed from the persistence message store",
                persistMessage.Id,
                persistMessage.DeliveryType
            );
        }

        await MarkAsDeliveredAsync(persistMessage.MessageId, cancellationToken).ConfigureAwait(false);
    }
}
