using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Core.Events;
using MassTransit;

namespace BuildingBlocks.Integration.MassTransit;

public class MassTransitBus(
    ISendEndpointProvider sendEndpointProvider,
    IPublishEndpoint publishEndpoint,
    IMessageMetadataAccessor messageMetadataAccessor
) : IExternalEventBus
{
    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : IMessage
    {
        var correlationId = messageMetadataAccessor.GetCorrelationId();
        var cautionId = messageMetadataAccessor.GetCorrelationId();
        var envelopeMessage = EventEnvelope.From(message, correlationId, cautionId);

        return PublishAsync(envelopeMessage, cancellationToken);
    }

    public async Task PublishAsync(IEventEnvelope eventEnvelope, CancellationToken cancellationToken = default)
    {
        await publishEndpoint.Publish(
            eventEnvelope,
            envelopeWrapperContext => FillMasstransitContextInformation(eventEnvelope, envelopeWrapperContext),
            cancellationToken
        );
    }

    public async Task PublishAsync(
        IEventEnvelope eventEnvelope,
        string? exchangeOrTopic = null,
        string? queue = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrEmpty(queue) && string.IsNullOrEmpty(exchangeOrTopic))
        {
            await PublishAsync(eventEnvelope, cancellationToken);
            return;
        }

        // Ref: https://stackoverflow.com/a/60269493/581476
        string endpointAddress = GetEndpointAddress(exchangeOrTopic, queue);

        var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri(endpointAddress));
        await sendEndpoint.Send(
            eventEnvelope,
            envelopeWrapperContext => FillMasstransitContextInformation(eventEnvelope, envelopeWrapperContext),
            cancellationToken
        );
    }

    private static void FillMasstransitContextInformation(
        IEventEnvelope eventEnvelope,
        PublishContext<IEventEnvelope> envelopeWrapperContext
    )
    {
        if (eventEnvelope.Metadata is null)
        {
            return;
        }

        // https://masstransit.io/documentation/concepts/messages#message-headers
        // https://www.enterpriseintegrationpatterns.com/patterns/messaging/EnvelopeWrapper.html
        // Just for filling masstransit related field, but we have a separated envelope message.
        envelopeWrapperContext.MessageId = eventEnvelope.Metadata.MessageId;
        envelopeWrapperContext.CorrelationId = eventEnvelope.Metadata.CorrelationId;

        foreach (var header in eventEnvelope.Metadata.Headers)
        {
            envelopeWrapperContext.Headers.Set(header.Key, header.Value);
        }
    }

    private static void FillMasstransitContextInformation(
        IEventEnvelope eventEnvelope,
        SendContext<IEventEnvelope> envelopeWrapperContext
    )
    {
        if (eventEnvelope.Metadata is null)
        {
            return;
        }

        // https://masstransit.io/documentation/concepts/messages#message-headers
        // https://www.enterpriseintegrationpatterns.com/patterns/messaging/EnvelopeWrapper.html
        // Just for filling masstransit related field, but we have a separated envelope message.
        envelopeWrapperContext.MessageId = eventEnvelope.Metadata.MessageId;
        envelopeWrapperContext.CorrelationId = eventEnvelope.Metadata.CorrelationId;

        foreach (var header in eventEnvelope.Metadata.Headers)
        {
            envelopeWrapperContext.Headers.Set(header.Key, header.Value);
        }
    }

    private static string GetEndpointAddress(string? exchangeOrTopic, string? queue)
    {
        return !string.IsNullOrEmpty(queue) && !string.IsNullOrEmpty(exchangeOrTopic)
            ? $"exchange:{exchangeOrTopic}?bind=true&queue={queue}"
            : !string.IsNullOrEmpty(queue)
                ? $"queue={queue}"
                : $"exchange:{exchangeOrTopic}";
    }

    public void Consume<TMessage>(
        IMessageHandler<TMessage> handler,
        Action<IConsumeConfigurationBuilder>? consumeBuilder = null
    )
        where TMessage : IMessage { }

    public Task Consume<TMessage>(
        Abstractions.Messaging.MessageHandler<TMessage> subscribeMethod,
        Action<IConsumeConfigurationBuilder>? consumeBuilder = null,
        CancellationToken cancellationToken = default
    )
        where TMessage : IMessage
    {
        return Task.CompletedTask;
    }

    public Task Consume<TMessage>(CancellationToken cancellationToken = default)
        where TMessage : IMessage
    {
        return Task.CompletedTask;
    }

    public Task Consume(Type messageType, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task Consume<THandler, TMessage>(CancellationToken cancellationToken = default)
        where THandler : IMessageHandler<TMessage>
        where TMessage : IMessage
    {
        return Task.CompletedTask;
    }

    public Task ConsumeAll(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task ConsumeAllFromAssemblyOf<TType>(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task ConsumeAllFromAssemblyOf(
        CancellationToken cancellationToken = default,
        params Type[] assemblyMarkerTypes
    )
    {
        return Task.CompletedTask;
    }
}
