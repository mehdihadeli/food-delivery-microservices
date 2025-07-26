using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Abstractions.Messages.MessagePersistence;
using BuildingBlocks.Core;
using BuildingBlocks.Core.Constants;
using BuildingBlocks.Core.Messages;
using Humanizer;
using Microsoft.Extensions.Options;
using MessageHeaders = BuildingBlocks.Core.Messages.MessageHeaders;

namespace BuildingBlocks.Integration.MassTransit;

public class MassTransitBus(
    IBusDirectPublisher busDirectPublisher,
    IMessageMetadataAccessor messageMetadataAccessor,
    IMessagePersistenceService messagePersistenceService,
    IOptions<MessagingOptions> messagingOptions
) : IExternalEventBus
{
    private readonly MessagingOptions _messagingOptions = messagingOptions.Value;

    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
    {
        var correlationId = messageMetadataAccessor.GetCorrelationId();
        var messageTypeName = message.GetType().Name.Underscore();
        var cautionId = message.MessageId;

        var eventEnvelope = MessageEnvelopeFactory.From(
            message,
            correlationId,
            cautionId,
            new Dictionary<string, object?>
            {
                { MessageHeaders.Name, message.GetType().Name.Underscore() },
                { MessageHeaders.ExchangeOrTopic, $"{messageTypeName}{MessagingConstants.PrimaryExchangePostfix}" },
                { MessageHeaders.Queue, messageTypeName },
            }
        );

        return PublishAsync(eventEnvelope, cancellationToken);
    }

    public async Task PublishAsync<TMessage>(
        IMessageEnvelope<TMessage> messageEnvelope,
        CancellationToken cancellationToken = default
    )
        where TMessage : class, IMessage
    {
        if (_messagingOptions.OutboxEnabled)
        {
            await messagePersistenceService
                .AddPublishMessageAsync(messageEnvelope, cancellationToken)
                .ConfigureAwait(false);
            return;
        }

        await busDirectPublisher.PublishAsync(messageEnvelope, cancellationToken).ConfigureAwait(false);
    }

    public async Task PublishAsync(IMessageEnvelopeBase messageEnvelope, CancellationToken cancellationToken = default)
    {
        if (_messagingOptions.OutboxEnabled)
        {
            await messagePersistenceService
                .AddPublishMessageAsync(messageEnvelope, cancellationToken)
                .ConfigureAwait(false);
            return;
        }

        await busDirectPublisher.PublishAsync(messageEnvelope, cancellationToken).ConfigureAwait(false);
    }

    public async Task PublishAsync<TMessage>(
        TMessage message,
        string? exchangeOrTopic = null,
        string? queue = null,
        CancellationToken cancellationToken = default
    )
        where TMessage : class, IMessage
    {
        var correlationId = messageMetadataAccessor.GetCorrelationId();
        var cautionId = message.MessageId;
        var messageTypeName = message.GetType().Name.Underscore();

        var eventEnvelope = MessageEnvelopeFactory.From<TMessage>(
            message,
            correlationId,
            cautionId,
            new Dictionary<string, object?>
            {
                { MessageHeaders.Name, message.GetType().Name.Underscore() },
                {
                    MessageHeaders.ExchangeOrTopic,
                    exchangeOrTopic ?? $"{messageTypeName}{MessagingConstants.PrimaryExchangePostfix}"
                },
                { MessageHeaders.Queue, queue ?? messageTypeName },
            }
        );

        if (_messagingOptions.OutboxEnabled)
        {
            await messagePersistenceService.AddPublishMessageAsync(eventEnvelope, cancellationToken);
            return;
        }

        await busDirectPublisher.PublishAsync(eventEnvelope, exchangeOrTopic, queue, cancellationToken);
    }

    public async Task PublishAsync<TMessage>(
        IMessageEnvelope<TMessage> messageEnvelope,
        string? exchangeOrTopic = null,
        string? queue = null,
        CancellationToken cancellationToken = default
    )
        where TMessage : class, IMessage
    {
        var messageTypeName = messageEnvelope.Message.GetType().Name.Underscore();

        if (_messagingOptions.OutboxEnabled)
        {
            await messagePersistenceService.AddPublishMessageAsync(messageEnvelope, cancellationToken);
            return;
        }

        await busDirectPublisher.PublishAsync(
            messageEnvelope,
            exchangeOrTopic ?? $"{messageTypeName}{MessagingConstants.PrimaryExchangePostfix}",
            queue ?? messageTypeName,
            cancellationToken
        );
    }
}
