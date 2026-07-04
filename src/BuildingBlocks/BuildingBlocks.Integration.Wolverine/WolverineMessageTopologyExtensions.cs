using BuildingBlocks.Core.Constants;
using Humanizer;
using Wolverine;
using Wolverine.RabbitMQ;

namespace BuildingBlocks.Integration.Wolverine;

public static class WolverineMessageTopologyExtensions
{
    private static readonly HashSet<RabbitMqListenerTopology> RegisteredListenerTopologies = [];
    private static readonly HashSet<RabbitMqPublishTopology> RegisteredPublishTopologies = [];
    private static readonly Lock TopologyLock = new();

    public static void PublishToPrimaryExchange<TMessage>(this WolverineOptions options)
        where TMessage : class
    {
        var messageName = typeof(TMessage).Name.Underscore();
        var exchangeName = $"{messageName}{MessagingConstants.PrimaryExchangePostfix}";

        RegisterPublishTopology(exchangeName);

        options
            .PublishMessage<TMessage>()
            .ToRabbitRoutingKey(
                exchangeName,
                messageName,
                exchange =>
                {
                    exchange.ExchangeType = ExchangeType.Direct;
                    exchange.IsDurable = true;
                }
            );
    }

    public static void ListenToPrimaryExchange<TMessage>(this WolverineOptions options, string? queueName = null)
        where TMessage : class
    {
        var messageName = typeof(TMessage).Name.Underscore();
        var exchangeName = $"{messageName}{MessagingConstants.PrimaryExchangePostfix}";
        queueName ??= messageName;

        RegisterListenerTopology(queueName, exchangeName, messageName);

        options
            .ConfigureRabbitMq()
            .DeclareQueue(
                queueName,
                queue =>
                {
                    queue.IsDurable = true;
                }
            );

        options
            .ConfigureRabbitMq()
            .BindExchange(exchangeName, ExchangeType.Direct)
            .ToQueue(
                queueName,
                messageName,
                queue =>
                {
                    queue.IsDurable = true;
                }
            );

        options
            .ListenToRabbitQueue(
                queueName,
                queue =>
                {
                    queue.IsDurable = true;
                }
            )
            .UseDurableInbox();
    }

    internal static IReadOnlyCollection<RabbitMqListenerTopology> GetRegisteredListenerTopologies()
    {
        lock (TopologyLock)
        {
            return RegisteredListenerTopologies.ToArray();
        }
    }

    internal static IReadOnlyCollection<RabbitMqPublishTopology> GetRegisteredPublishTopologies()
    {
        lock (TopologyLock)
        {
            return RegisteredPublishTopologies.ToArray();
        }
    }

    private static void RegisterListenerTopology(string queueName, string exchangeName, string routingKey)
    {
        lock (TopologyLock)
        {
            RegisteredListenerTopologies.Add(new RabbitMqListenerTopology(queueName, exchangeName, routingKey));
        }
    }

    private static void RegisterPublishTopology(string exchangeName)
    {
        lock (TopologyLock)
        {
            RegisteredPublishTopologies.Add(new RabbitMqPublishTopology(exchangeName));
        }
    }
}

internal sealed record RabbitMqListenerTopology(string QueueName, string ExchangeName, string RoutingKey);

internal sealed record RabbitMqPublishTopology(string ExchangeName);
