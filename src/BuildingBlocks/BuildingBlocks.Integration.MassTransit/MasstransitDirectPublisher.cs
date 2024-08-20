using System.Reflection;
using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Core.Messaging;
using Humanizer;
using MassTransit;
using RabbitMQ.Client;

namespace BuildingBlocks.Integration.MassTransit;

public class MasstransitDirectPublisher(IBus bus) : IBusDirectPublisher
{
    public async Task PublishAsync<TMessage>(
        IEventEnvelope<TMessage> eventEnvelope,
        CancellationToken cancellationToken = default
    )
        where TMessage : IMessage
    {
        // https://github.com/MassTransit/MassTransit/blob/eb3c9ee1007cea313deb39dc7c4eb796b7e61579/src/MassTransit/SqlTransport/SqlTransport/ConnectionContextSupervisor.cs#L35
        await bus.Publish(
            eventEnvelope,
            envelopeWrapperContext => FillMasstransitContextInformation(eventEnvelope, envelopeWrapperContext),
            cancellationToken
        );
    }

    public Task PublishAsync(IEventEnvelope eventEnvelope, CancellationToken cancellationToken = default)
    {
        var messageType = eventEnvelope.Message.GetType();

        MethodInfo publishMethod = typeof(IBusDirectPublisher)
            .GetMethods()
            .FirstOrDefault(x => x.GetGenericArguments().Length != 0 && x.GetParameters().Length == 2)!;
        MethodInfo genericPublishMethod = publishMethod.MakeGenericMethod(messageType);

        Task publishTask = (Task)genericPublishMethod.Invoke(this, new object[] { eventEnvelope, cancellationToken });

        return publishTask!;
    }

    public async Task PublishAsync<TMessage>(
        IEventEnvelope<TMessage> eventEnvelope,
        string? exchangeOrTopic = null,
        string? queue = null,
        CancellationToken cancellationToken = default
    )
        where TMessage : IMessage
    {
        var bindExchangeName = eventEnvelope.Message.GetType().Name.Underscore();

        if (string.IsNullOrEmpty(exchangeOrTopic))
        {
            exchangeOrTopic = $"{bindExchangeName}{MessagingConstants.PrimaryExchangePostfix}";
        }

        // Ref: https://stackoverflow.com/a/60269493/581476
        string endpointAddress = GetEndpointAddress(
            exchangeOrTopic: exchangeOrTopic,
            queue: queue,
            bindExchange: bindExchangeName,
            exchangeType: ExchangeType.Direct
        );

        var sendEndpoint = await bus.GetSendEndpoint(new Uri(endpointAddress));
        // https://github.com/MassTransit/MassTransit/blob/eb3c9ee1007cea313deb39dc7c4eb796b7e61579/src/MassTransit/SqlTransport/SqlTransport/ConnectionContextSupervisor.cs#L53
        await sendEndpoint.Send(
            eventEnvelope,
            envelopeWrapperContext => FillMasstransitContextInformation(eventEnvelope, envelopeWrapperContext),
            cancellationToken
        );
    }

    public Task PublishAsync(
        IEventEnvelope eventEnvelope,
        string? exchangeOrTopic = null,
        string? queue = null,
        CancellationToken cancellationToken = default
    )
    {
        var messageType = eventEnvelope.Message.GetType();

        MethodInfo publishMethod = typeof(IBusDirectPublisher)
            .GetMethods()
            .FirstOrDefault(x => x.GetGenericArguments().Length != 0 && x.GetParameters().Length == 4)!;
        MethodInfo genericPublishMethod = publishMethod.MakeGenericMethod(messageType);

        Task publishTask = (Task)
            genericPublishMethod.Invoke(
                this,
                new object[] { eventEnvelope, exchangeOrTopic, queue, cancellationToken }
            );

        return publishTask!;
    }

    private static void FillMasstransitContextInformation(
        IEventEnvelope eventEnvelope,
        PublishContext<IEventEnvelope> envelopeWrapperContext
    )
    {
        // https://masstransit.io/documentation/concepts/messages#message-headers
        // https://www.enterpriseintegrationpatterns.com/patterns/messaging/EnvelopeWrapper.html
        // Just for filling masstransit related field, but we have a separated envelope message.
        envelopeWrapperContext.MessageId = eventEnvelope.Metadata.MessageId;
        envelopeWrapperContext.CorrelationId = eventEnvelope.Metadata.CorrelationId;
        envelopeWrapperContext.SetRoutingKey(eventEnvelope.Message.GetType().Name.Underscore());

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

    private static string GetEndpointAddress(
        string exchangeOrTopic,
        string? queue,
        string? bindExchange,
        string? exchangeType = ExchangeType.Direct,
        bool bindQueue = false
    )
    {
        // https://masstransit.io/documentation/concepts/producers#short-addresses
        // https://github.com/MassTransit/MassTransit/blob/ac44867da9d7a93bb7d330680586af123c1ee0b7/src/Transports/MassTransit.RabbitMqTransport/RabbitMqEndpointAddress.cs#L63
        // https://github.com/MassTransit/MassTransit/blob/ac44867da9d7a93bb7d330680586af123c1ee0b7/src/Transports/MassTransit.RabbitMqTransport/RabbitMqEndpointAddress.cs#L98
        // Start with the base address
        string endpoint = $"exchange:{exchangeOrTopic}?type={exchangeType}&durable=true";

        // If there is a bindExchange, add it to the query parameters
        if (!string.IsNullOrEmpty(bindExchange))
        {
            endpoint += $"&bindexchange={bindExchange}";
        }

        if (!string.IsNullOrEmpty(queue))
        {
            endpoint += $"&queue={queue}";
        }

        if (bindQueue)
        {
            endpoint += "&bind=true";
        }

        return endpoint;
    }
}
