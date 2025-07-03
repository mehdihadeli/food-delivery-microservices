using BuildingBlocks.Integration.MassTransit;
using FoodDelivery.Services.Shared.Identity.Users.Events.Integration.v1;
using Humanizer;
using MassTransit;
using RabbitMQ.Client;

namespace FoodDelivery.Services.Shared.Identity.Users;

public static class MassTransitExtensions
{
    public static void ConfigureUserPublishMessagesTopology(this IRabbitMqBusFactoryConfigurator cfg)
    {
        // https://masstransit.io/documentation/transports/rabbitmq
        cfg.Message<UserRegisteredV1>(e =>
        {
            // https://masstransit.io/documentation/configuration/topology/message
            // Name of the `primary exchange` for type based message publishing and sending
            // e.SetEntityName($"{nameof(UserRegisteredV1).Underscore()}{MessagingConstants.PrimaryExchangePostfix}");
            e.SetEntityNameFormatter(new CustomEntityNameFormatter<UserRegisteredV1>());
        });

        // configuration for MessagePublishTopologyConfiguration and using IPublishEndpoint
        cfg.Publish<UserRegisteredV1>(e =>
        {
            // setup primary exchange
            e.Durable = true;
            e.ExchangeType = ExchangeType.Direct;
        });

        // configuration for MessageSendTopologyConfiguration and using ISendEndpointProvider
        cfg.Send<UserRegisteredV1>(e =>
        {
            // route by message type to binding fanout exchange (exchange to exchange binding)
            e.UseRoutingKeyFormatter(context => context.Message.GetType().Name.Underscore());
        });

        cfg.SetQueueArgument("x-dead-letter-exchange", $"{nameof(UserRegisteredV1).Underscore()}_dead_letter_exchange");
        cfg.SetQueueArgument("x-dead-letter-routing-key", nameof(UserRegisteredV1).Underscore());

        cfg.Message<UserStateUpdatedV1>(e =>
        {
            // https://masstransit.io/documentation/configuration/topology/message
            // Name of the `primary exchange` for type based message publishing and sending
            // e.SetEntityName($"{nameof(UserStateUpdatedV1).Underscore()}{MessagingConstants.PrimaryExchangePostfix}");
            e.SetEntityNameFormatter(new CustomEntityNameFormatter<UserStateUpdatedV1>());
        });

        cfg.Publish<UserStateUpdatedV1>(e =>
        {
            // we configured some shared settings for all publish message in masstransit publish topologies

            // setup primary exchange
            e.Durable = true;
            e.ExchangeType = ExchangeType.Direct;
        });

        // configuration for MessageSendTopologyConfiguration and using ISendEndpointProvider
        cfg.Send<UserStateUpdatedV1>(e =>
        {
            // route by message type to binding fanout exchange (exchange to exchange binding)
            e.UseRoutingKeyFormatter(context => context.Message.GetType().Name.Underscore());
        });

        cfg.SetQueueArgument(
            "x-dead-letter-exchange",
            $"{nameof(UserStateUpdatedV1).Underscore()}_dead_letter_exchange"
        );
        cfg.SetQueueArgument("x-dead-letter-routing-key", nameof(UserStateUpdatedV1).Underscore());
    }
}
