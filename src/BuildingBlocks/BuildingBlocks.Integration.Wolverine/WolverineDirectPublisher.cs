using System.Reflection;
using BuildingBlocks.Abstractions.Messages;
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
        await bus.PublishAsync(messageEnvelope.Message, WolverineDeliveryOptionsFactory.Build(messageEnvelope))
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
            .SendAsync(messageEnvelope.Message, WolverineDeliveryOptionsFactory.Build(messageEnvelope))
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

    private Uri GetEndpointAddress(string exchangeOrTopic, string? queue, string routingKey)
    {
        return WolverineEndpointAddressFactory.Get(
            exchangeOrTopic,
            queue,
            routingKey,
            _wolverineBusOptions.ConfigureConsumeTopology
        );
    }
}
