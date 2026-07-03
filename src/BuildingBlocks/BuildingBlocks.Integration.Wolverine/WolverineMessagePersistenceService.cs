using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Abstractions.Messages.MessagePersistence;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Messages;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using BuildingMessage = BuildingBlocks.Abstractions.Messages.IMessage;
using Constants = BuildingBlocks.Core.Constants;

namespace BuildingBlocks.Integration.Wolverine;

public class WolverineMessagePersistenceService(
    IMessageBus bus,
    IServiceProvider serviceProvider,
    ILogger<WolverineMessagePersistenceService> logger
) : IMessagePersistenceService
{
    public async Task AddPublishMessageAsync(
        IMessageEnvelopeBase messageEnvelope,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(messageEnvelope);

        TryEnrollCurrentDbContext();

        await PublishEnvelopeAsync(messageEnvelope).ConfigureAwait(false);

        logger.LogInformation(
            "Published message {MessageType} through Wolverine durable messaging.",
            messageEnvelope.Message.GetType().Name
        );
    }

    public Task AddReceivedMessageAsync<TMessage>(
        IMessageEnvelopeBase messageEnvelope,
        Func<IMessageEnvelopeBase, Task> dispatchAction,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(dispatchAction);
        return dispatchAction(messageEnvelope);
    }

    public async Task AddInternalMessageAsync<TInternalCommand>(
        TInternalCommand internalCommand,
        CancellationToken cancellationToken = default
    )
        where TInternalCommand : IInternalCommand
    {
        ArgumentNullException.ThrowIfNull(internalCommand);

        TryEnrollCurrentDbContext();

        await bus.SendAsync(internalCommand).ConfigureAwait(false);

        logger.LogInformation(
            "Scheduled internal command {CommandType} through Wolverine durable local queues.",
            typeof(TInternalCommand).Name
        );
    }

    public async Task AddNotificationAsync<TDomainNotification>(
        TDomainNotification notification,
        CancellationToken cancellationToken = default
    )
        where TDomainNotification : IDomainNotificationEvent<IDomainEvent>
    {
        ArgumentNullException.ThrowIfNull(notification);

        TryEnrollCurrentDbContext();

        await bus.SendAsync(notification).ConfigureAwait(false);

        logger.LogInformation(
            "Scheduled domain notification {NotificationType} through Wolverine durable local queues.",
            typeof(TDomainNotification).Name
        );
    }

    private async Task PublishEnvelopeAsync(IMessageEnvelopeBase messageEnvelope)
    {
        var publishMethod = typeof(WolverineMessagePersistenceService).GetMethod(
            nameof(PublishEnvelopeAsyncCore),
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
        )!;

        var genericMethod = publishMethod.MakeGenericMethod(messageEnvelope.Message.GetType());
        await (Task)genericMethod.Invoke(this, [messageEnvelope])!;
    }

    private async Task PublishEnvelopeAsyncCore<TMessage>(IMessageEnvelopeBase messageEnvelope)
        where TMessage : class, BuildingMessage
    {
        var message = (TMessage)messageEnvelope.Message;
        var messageTypeName = message.GetType().Name.Underscore();
        var exchangeOrTopic = messageEnvelope.Metadata.Headers.TryGetValue(
            MessageHeaders.ExchangeOrTopic,
            out var exchangeValue
        )
            ? exchangeValue?.ToString()
            : null;
        var queue = messageEnvelope.Metadata.Headers.TryGetValue(MessageHeaders.Queue, out var queueValue)
            ? queueValue?.ToString()
            : null;

        if (!string.IsNullOrWhiteSpace(exchangeOrTopic) || !string.IsNullOrWhiteSpace(queue))
        {
            exchangeOrTopic ??= $"{messageTypeName}{Constants.MessagingConstants.PrimaryExchangePostfix}";
            var endpointAddress = WolverineEndpointAddressFactory.Get(
                exchangeOrTopic,
                queue,
                messageTypeName,
                configureConsumeTopology: false
            );

            await bus.EndpointFor(endpointAddress)
                .SendAsync(message, WolverineDeliveryOptionsFactory.Build(messageEnvelope))
                .ConfigureAwait(false);
            return;
        }

        await bus.PublishAsync(message, WolverineDeliveryOptionsFactory.Build(messageEnvelope)).ConfigureAwait(false);
    }

    private void TryEnrollCurrentDbContext()
    {
        var outbox = serviceProvider.GetService<IDbContextOutbox>();
        if (outbox is null)
        {
            return;
        }

        if (serviceProvider.GetService<IDbFacadeResolver>() is DbContext dbContext)
        {
            outbox.Enroll(dbContext);
        }
    }
}
