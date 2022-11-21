using ECommerce.Services.Shared.Customers.RestockSubscriptions.Events.v1.Integration;
using Humanizer;
using MassTransit;
using RabbitMQ.Client;

namespace ECommerce.Services.Customers.RestockSubscriptions;

internal static class MassTransitExtensions
{
    internal static void AddRestockSubscriptionPublishers(this IRabbitMqBusFactoryConfigurator cfg)
    {
        cfg.Message<RestockSubscriptionCreatedV1>(e =>
            e.SetEntityName(
                $"{nameof(RestockSubscriptionCreatedV1).Underscore()}.input_exchange")); // name of the primary exchange
        cfg.Publish<RestockSubscriptionCreatedV1>(e => e.ExchangeType = ExchangeType.Direct); // primary exchange type
        cfg.Send<RestockSubscriptionCreatedV1>(e =>
        {
            // route by message type to binding fanout exchange (exchange to exchange binding)
            e.UseRoutingKeyFormatter(context =>
                context.Message.GetType().Name.Underscore());
        });
    }
}
