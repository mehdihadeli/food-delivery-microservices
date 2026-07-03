using System.Globalization;
using System.Reflection;
using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Core.Messages;
using Humanizer;
using Microsoft.Extensions.Options;
using Wolverine;
using Wolverine.RabbitMQ;
using BuildingMessage = BuildingBlocks.Abstractions.Messages.IMessage;
using Constants = BuildingBlocks.Core.Constants;
using MessageHeaders = BuildingBlocks.Core.Messages.MessageHeaders;

namespace BuildingBlocks.Integration.Wolverine;

public class WolverineDirectPublisher(IMessageBus bus, IOptions<WolverineBusOptions> wolverineBusOptions)
    : IBusDirectPublisher
{
    private readonly WolverineBusOptions _wolverineBusOptions = wolverineBusOptions.Value;

    public async Task PublishAsync<TMessage>(
        IMessageEnvelope<TMessage> messageEnvelope,
        CancellationToken cancellationToken = default
    )
        where TMessage : class, BuildingMessage
    {
        await bus.PublishAsync(messageEnvelope.Message, BuildDeliveryOptions(messageEnvelope)).ConfigureAwait(false);
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
        where TMessage : class, BuildingMessage
    {
        var routingKey = messageEnvelope.Message.GetType().Name.Underscore();

        if (string.IsNullOrEmpty(exchangeOrTopic))
        {
            exchangeOrTopic = $"{routingKey}{Constants.MessagingConstants.PrimaryExchangePostfix}";
        }

        var endpointAddress = GetEndpointAddress(
            exchangeOrTopic: exchangeOrTopic,
            queue: queue,
            routingKey: routingKey
        );

        await bus.EndpointFor(endpointAddress)
            .SendAsync(messageEnvelope.Message, BuildDeliveryOptions(messageEnvelope))
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

    private static DeliveryOptions BuildDeliveryOptions<TMessage>(IMessageEnvelope<TMessage> messageEnvelope)
        where TMessage : class, BuildingMessage
    {
        var deliveryOptions = new DeliveryOptions
        {
            CorrelationId = messageEnvelope.Metadata.CorrelationId.ToString(),
            CausationId = messageEnvelope.Metadata.CausationId?.ToString(),
        };

        deliveryOptions.WithHeader(MessageHeaders.MessageId, messageEnvelope.Metadata.MessageId.ToString());
        deliveryOptions.WithHeader(MessageHeaders.Type, messageEnvelope.Metadata.MessageType);
        deliveryOptions.WithHeader(MessageHeaders.Name, messageEnvelope.Metadata.Name);
        deliveryOptions.WithHeader(MessageHeaders.CausationId, messageEnvelope.Metadata.CausationId.ToString());
        deliveryOptions.WithHeader(MessageHeaders.CorrelationId, messageEnvelope.Metadata.CorrelationId.ToString());
        deliveryOptions.WithHeader(
            MessageHeaders.Created,
            messageEnvelope.Metadata.Created.ToString(CultureInfo.InvariantCulture)
        );

        foreach (var header in messageEnvelope.Metadata.Headers)
        {
            if (header.Value is not null)
            {
                deliveryOptions.WithHeader(header.Key, header.Value.ToString()!);
            }
        }

        return deliveryOptions;
    }

    private Uri GetEndpointAddress(string exchangeOrTopic, string? queue, string routingKey)
    {
        if (!_wolverineBusOptions.ConfigureConsumeTopology || !string.IsNullOrWhiteSpace(queue))
        {
            return RabbitMqEndpointUri.Routing(exchangeOrTopic, queue ?? routingKey);
        }

        return RabbitMqEndpointUri.Exchange(exchangeOrTopic);
    }
}
