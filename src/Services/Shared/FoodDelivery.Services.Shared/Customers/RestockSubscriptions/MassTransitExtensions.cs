using BuildingBlocks.Integration.MassTransit;
using FoodDelivery.Services.Shared.Customers.RestockSubscriptions.Events.Integration.v1;
using Humanizer;
using MassTransit;
using RabbitMQ.Client;

namespace FoodDelivery.Services.Shared.Customers.RestockSubscriptions;

public static class MassTransitExtensions
{
    public static void ConfigureRestockSubscriptionPublishMessagesTopology(this IRabbitMqBusFactoryConfigurator cfg)
    {
        cfg.Message<RestockSubscriptionCreatedV1>(e =>
        {
            // we configured some shared settings for all publish message in masstransit publish topologies

            // name of the primary exchange
            // https://masstransit.io/documentation/configuration/topology/message
            // e.SetEntityName(
            //     $"{nameof(RestockSubscriptionCreatedV1).Underscore()}{MessagingConstants.PrimaryExchangePostfix}"
            // );
            e.SetEntityNameFormatter(new CustomEntityNameFormatter<RestockSubscriptionCreatedV1>());
        });

        cfg.Publish<RestockSubscriptionCreatedV1>(e =>
        {
            // primary exchange type
            e.ExchangeType = ExchangeType.Direct;
            e.Durable = true;
        });

        cfg.Send<RestockSubscriptionCreatedV1>(e =>
        {
            // route by message type to binding fanout exchange (exchange to exchange binding)
            e.UseRoutingKeyFormatter(context => context.Message.GetType().Name.Underscore());
        });

        cfg.SetQueueArgument(
            "x-dead-letter-exchange",
            $"{nameof(RestockSubscriptionCreatedV1).Underscore()}_dead_letter_exchange"
        );
        cfg.SetQueueArgument("x-dead-letter-routing-key", nameof(RestockSubscriptionCreatedV1).Underscore());
    }
}
