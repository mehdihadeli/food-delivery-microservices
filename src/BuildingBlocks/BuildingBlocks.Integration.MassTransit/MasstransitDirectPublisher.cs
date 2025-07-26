using System.Globalization;
using System.Reflection;
using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Core.Messages;
using Humanizer;
using MassTransit;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Constants = BuildingBlocks.Core.Constants;
using MessageHeaders = BuildingBlocks.Core.Messages.MessageHeaders;

namespace BuildingBlocks.Integration.MassTransit;

public class MasstransitDirectPublisher(IBus bus, IOptions<MasstransitOptions> masstransitOptions) : IBusDirectPublisher
{
    private readonly MasstransitOptions _masstransitOptions = masstransitOptions.Value;

    public async Task PublishAsync<TMessage>(
        IMessageEnvelope<TMessage> messageEnvelope,
        CancellationToken cancellationToken = default
    )
        where TMessage : class, IMessage
    {
        // https://github.com/MassTransit/MassTransit/blob/eb3c9ee1007cea313deb39dc7c4eb796b7e61579/src/MassTransit/SqlTransport/SqlTransport/ConnectionContextSupervisor.cs#L35
        await bus.Publish(
                messageEnvelope.Message,
                publishContext => FillMasstransitContextInformation(messageEnvelope, publishContext),
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public Task PublishAsync(IMessageEnvelopeBase messageEnvelope, CancellationToken cancellationToken = default)
    {
        var messageType = messageEnvelope.Message.GetType();

        MethodInfo publishMethod = typeof(IBusDirectPublisher)
            .GetMethods()
            .FirstOrDefault(x => x.GetGenericArguments().Length != 0 && x.GetParameters().Length == 2)!;
        MethodInfo genericPublishMethod = publishMethod.MakeGenericMethod(messageType);

        Task publishTask = (Task)genericPublishMethod.Invoke(this, new object[] { messageEnvelope, cancellationToken });

        return publishTask!;
    }

    public async Task PublishAsync<TMessage>(
        IMessageEnvelope<TMessage> messageEnvelope,
        string? exchangeOrTopic = null,
        string? queue = null,
        CancellationToken cancellationToken = default
    )
        where TMessage : class, IMessage
    {
        var bindExchangeName = messageEnvelope.Message.GetType().Name.Underscore();

        if (string.IsNullOrEmpty(exchangeOrTopic))
        {
            exchangeOrTopic = $"{bindExchangeName}{Constants.MessagingConstants.PrimaryExchangePostfix}";
        }

        // Ref: https://stackoverflow.com/a/60269493/581476
        string endpointAddress = GetEndpointAddress(
            exchangeOrTopic: exchangeOrTopic,
            queue: queue,
            bindExchange: bindExchangeName,
            exchangeType: ExchangeType.Direct
        );

        var sendEndpoint = await bus.GetSendEndpoint(new Uri(endpointAddress)).ConfigureAwait(false);
        // https://github.com/MassTransit/MassTransit/blob/eb3c9ee1007cea313deb39dc7c4eb796b7e61579/src/MassTransit/SqlTransport/SqlTransport/ConnectionContextSupervisor.cs#L53
        await sendEndpoint
            .Send(
                messageEnvelope.Message,
                sendContext => FillMasstransitContextInformation(messageEnvelope, sendContext),
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public Task PublishAsync(
        IMessageEnvelopeBase messageEnvelope,
        string? exchangeOrTopic = null,
        string? queue = null,
        CancellationToken cancellationToken = default
    )
    {
        var messageType = messageEnvelope.Message.GetType();

        MethodInfo publishMethod = typeof(IBusDirectPublisher)
            .GetMethods()
            .FirstOrDefault(x => x.GetGenericArguments().Length != 0 && x.GetParameters().Length == 4)!;
        MethodInfo genericPublishMethod = publishMethod.MakeGenericMethod(messageType);

        Task publishTask = (Task)
            genericPublishMethod.Invoke(
                this,
                new object[] { messageEnvelope, exchangeOrTopic, queue, cancellationToken }
            );

        return publishTask!;
    }

    private void FillMasstransitContextInformation<TMessage>(
        IMessageEnvelope<TMessage> messageEnvelope,
        PublishContext<TMessage> envelopeWrapperContext
    )
        where TMessage : class, IMessage
    {
        // https://masstransit.io/documentation/concepts/messages#message-headers
        // https://www.enterpriseintegrationpatterns.com/patterns/messaging/EnvelopeWrapper.html
        // Just for filling masstransit related field, but we have a separated envelope message.
        envelopeWrapperContext.MessageId = messageEnvelope.Metadata.MessageId;
        envelopeWrapperContext.CorrelationId = messageEnvelope.Metadata.CorrelationId;

        if (!_masstransitOptions.ConfigureConsumeTopology)
        {
            // if `ConfigureConsumeTopology` is false we use direct exchange with message type name as routing-key, otherwise we use direct exchange with null routing-key (default exchange)
            envelopeWrapperContext.SetRoutingKey(messageEnvelope.Message.GetType().Name.Underscore());
        }

        envelopeWrapperContext.Headers.Set(MessageHeaders.Type, messageEnvelope.Metadata.MessageType);
        envelopeWrapperContext.Headers.Set(MessageHeaders.Name, messageEnvelope.Metadata.Name);
        envelopeWrapperContext.Headers.Set(MessageHeaders.CausationId, messageEnvelope.Metadata.CausationId);
        envelopeWrapperContext.Headers.Set(MessageHeaders.CorrelationId, messageEnvelope.Metadata.CorrelationId);
        envelopeWrapperContext.Headers.Set(
            MessageHeaders.Created,
            messageEnvelope.Metadata.Created.ToString(CultureInfo.InvariantCulture)
        );
        foreach (var header in messageEnvelope.Metadata.Headers)
        {
            envelopeWrapperContext.Headers.Set(header.Key, header.Value);
        }
    }

    private void FillMasstransitContextInformation<TMessage>(
        IMessageEnvelope<TMessage> messageEnvelope,
        SendContext<TMessage> envelopeWrapperContext
    )
        where TMessage : class, IMessage
    {
        // https://masstransit.io/documentation/concepts/messages#message-headers
        // https://www.enterpriseintegrationpatterns.com/patterns/messaging/EnvelopeWrapper.html
        // Just for filling masstransit related field, but we have a separated envelope message.
        envelopeWrapperContext.MessageId = messageEnvelope.Metadata.MessageId;
        envelopeWrapperContext.CorrelationId = messageEnvelope.Metadata.CorrelationId;
        if (!_masstransitOptions.ConfigureConsumeTopology)
        {
            // if `ConfigureConsumeTopology` is false we use direct exchange with message type name as routing-key, otherwise we use direct exchange with null routing-key (default exchange)
            envelopeWrapperContext.SetRoutingKey(messageEnvelope.Message.GetType().Name.Underscore());
        }

        envelopeWrapperContext.Headers.Set(MessageHeaders.Type, messageEnvelope.Metadata.MessageType);
        envelopeWrapperContext.Headers.Set(MessageHeaders.Name, messageEnvelope.Metadata.Name);
        envelopeWrapperContext.Headers.Set(MessageHeaders.CausationId, messageEnvelope.Metadata.CausationId);
        envelopeWrapperContext.Headers.Set(MessageHeaders.CorrelationId, messageEnvelope.Metadata.CorrelationId);
        envelopeWrapperContext.Headers.Set(
            MessageHeaders.Created,
            messageEnvelope.Metadata.Created.ToString(CultureInfo.InvariantCulture)
        );
        foreach (var header in messageEnvelope.Metadata.Headers)
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
