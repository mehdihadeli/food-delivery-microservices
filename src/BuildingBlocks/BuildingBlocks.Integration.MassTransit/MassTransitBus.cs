using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Web.Extenions;
using BuildingBlocks.Core.Web.HeaderPropagation;
using MassTransit;
using MassTransit.Serialization;

namespace BuildingBlocks.Integration.MassTransit;

public class MassTransitBus : IExternalEventBus
{
    private readonly ISendEndpointProvider _sendEndpointProvider;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly CustomHeaderPropagationStore _headerStore;

    public MassTransitBus(
        ISendEndpointProvider sendEndpointProvider,
        IPublishEndpoint publishEndpoint,
        CustomHeaderPropagationStore headerStore
    )
    {
        _sendEndpointProvider = sendEndpointProvider;
        _publishEndpoint = publishEndpoint;
        _headerStore = headerStore;
    }

    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
    {
        var correlationId = _headerStore.GetCorrelationId();
        var envelopeMessage = EventEnvelopeFactory.From<TMessage>(message, correlationId);

        return PublishAsync(envelopeMessage, cancellationToken);
    }

    public async Task PublishAsync<TMessage>(
        IEventEnvelope<TMessage> eventEnvelope,
        CancellationToken cancellationToken = default
    )
        where TMessage : class, IMessage
    {
        await _publishEndpoint.Publish(
            eventEnvelope,
            envelopeWrapperContext => ConfigMasstransitEnvelopeWrapper(eventEnvelope, envelopeWrapperContext),
            cancellationToken
        );
    }

    public async Task PublishAsync<TMessage>(
        IEventEnvelope<TMessage> eventEnvelope,
        string? exchangeOrTopic,
        string? queue,
        CancellationToken cancellationToken = default
    )
        where TMessage : class, IMessage
    {
        if (string.IsNullOrEmpty(queue) && string.IsNullOrEmpty(exchangeOrTopic))
        {
            await PublishAsync(eventEnvelope, cancellationToken);
            return;
        }

        // Ref: https://stackoverflow.com/a/60269493/581476
        string endpointAddress = GetEndpointAddress(exchangeOrTopic, queue);

        var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri(endpointAddress));
        await sendEndpoint.Send(
            eventEnvelope,
            envelopeWrapperContext => ConfigMasstransitEnvelopeWrapper(eventEnvelope, envelopeWrapperContext),
            cancellationToken
        );
    }

    public async Task PublishAsync(IEventEnvelope eventEnvelope, CancellationToken cancellationToken = default)
    {
        await _publishEndpoint.Publish(
            eventEnvelope,
            envelopeWrapperContext => ConfigMasstransitEnvelopeWrapper(eventEnvelope, envelopeWrapperContext),
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

        // if (message is IMessage data)
        // {
        //     meta = GetMetadata(data, meta);
        // }
        // else
        // {
        //     meta = GetMetadata(message, meta);
        // }

        // Ref: https://stackoverflow.com/a/60269493/581476
        string endpointAddress = GetEndpointAddress(exchangeOrTopic, queue);

        var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri(endpointAddress));
        await sendEndpoint.Send(
            eventEnvelope,
            envelopeWrapperContext => ConfigMasstransitEnvelopeWrapper(eventEnvelope, envelopeWrapperContext),
            cancellationToken
        );
    }

    private static void ConfigMasstransitEnvelopeWrapper(
        IEventEnvelope eventEnvelope,
        PublishContext<IEventEnvelope> envelopeWrapperContext
    )
    {
        // https://masstransit.io/documentation/concepts/messages#message-headers
        // https://www.enterpriseintegrationpatterns.com/patterns/messaging/EnvelopeWrapper.html
        // Just for filling masstransit related field but we have a separated envelope message.
        envelopeWrapperContext.MessageId = eventEnvelope.Metadata.MessageId;
        envelopeWrapperContext.CorrelationId = eventEnvelope.Metadata.CorrelationId;

        if (eventEnvelope.Metadata.Headers is not null)
        {
            foreach (var header in eventEnvelope.Metadata.Headers)
            {
                envelopeWrapperContext.Headers.Set(header.Key, header.Value);
            }
        }
    }

    private static void ConfigMasstransitEnvelopeWrapper(
        IEventEnvelope eventEnvelope,
        SendContext<IEventEnvelope> envelopeWrapperContext
    )
    {
        // https://masstransit.io/documentation/concepts/messages#message-headers
        // https://www.enterpriseintegrationpatterns.com/patterns/messaging/EnvelopeWrapper.html
        // Just for filling masstransit related field but we have a separated envelope message.
        envelopeWrapperContext.MessageId = eventEnvelope.Metadata.MessageId;
        envelopeWrapperContext.CorrelationId = eventEnvelope.Metadata.CorrelationId;

        if (eventEnvelope.Metadata.Headers is not null)
        {
            foreach (var header in eventEnvelope.Metadata.Headers)
            {
                envelopeWrapperContext.Headers.Set(header.Key, header.Value);
            }
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
        where TMessage : class, IMessage { }

    public Task Consume<TMessage>(
        Abstractions.Messaging.MessageHandler<TMessage> subscribeMethod,
        Action<IConsumeConfigurationBuilder>? consumeBuilder = null,
        CancellationToken cancellationToken = default
    )
        where TMessage : class, IMessage
    {
        return Task.CompletedTask;
    }

    public Task Consume<TMessage>(CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
    {
        return Task.CompletedTask;
    }

    public Task Consume(Type messageType, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task Consume<THandler, TMessage>(CancellationToken cancellationToken = default)
        where THandler : IMessageHandler<TMessage>
        where TMessage : class, IMessage
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
