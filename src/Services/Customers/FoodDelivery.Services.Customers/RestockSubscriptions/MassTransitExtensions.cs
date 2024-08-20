using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Core.Messaging;
using FoodDelivery.Services.Shared.Customers.RestockSubscriptions.Events.V1.Integration;
using Humanizer;
using MassTransit;

namespace FoodDelivery.Services.Customers.RestockSubscriptions;

internal static class MassTransitExtensions
{
    internal static void AddRestockSubscriptionPublishers(this IRabbitMqBusFactoryConfigurator cfg)
    {
        cfg.Message<IEventEnvelope<RestockSubscriptionCreatedV1>>(e =>
        {
            // we configured some shared settings for all publish message in masstransit publish topologies

            // name of the primary exchange
            e.SetEntityName(
                $"{nameof(RestockSubscriptionCreatedV1).Underscore()}{MessagingConstants.PrimaryExchangePostfix}"
            );
        });

        cfg.Publish<IEventEnvelope<RestockSubscriptionCreatedV1>>(e =>
        {
            // // we configured some shared settings for all publish message in masstransit publish topologies
            //
            // // primary exchange type
            // e.ExchangeType = ExchangeType.Direct;
            // e.Durable = true;
        });

        cfg.Send<IEventEnvelope<RestockSubscriptionCreatedV1>>(e =>
        {
            // route by message type to binding fanout exchange (exchange to exchange binding)
            e.UseRoutingKeyFormatter(context => context.Message.GetType().Name.Underscore());
        });
    }
}
