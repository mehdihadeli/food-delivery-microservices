using BuildingBlocks.Integration.MassTransit;
using FoodDelivery.Services.Shared.Customers.Customers.Events.Integration.v1;
using Humanizer;
using MassTransit;
using RabbitMQ.Client;

namespace FoodDelivery.Services.Customers.Customers;

public static class MassTransitExtensions
{
    public static void ConfigureCustomerPublishMessagesTopology(this IRabbitMqBusFactoryConfigurator cfg)
    {
        // https://masstransit.io/documentation/transports/rabbitmq
        cfg.Message<CustomerCreatedV1>(e =>
        {
            // https://masstransit.io/documentation/configuration/topology/message
            // Name of the `primary exchange` for type based message publishing and sending
            // e.SetEntityName($"{nameof(CustomerCreatedV1).Underscore()}{MessagingConstants.PrimaryExchangePostfix}");
            e.SetEntityNameFormatter(new CustomEntityNameFormatter<CustomerCreatedV1>());
        });

        // configuration for MessagePublishTopologyConfiguration and using IPublishEndpoint
        cfg.Publish<CustomerCreatedV1>(e =>
        {
            // setup primary exchange
            e.Durable = true;
            e.ExchangeType = ExchangeType.Direct;
        });

        // configuration for MessageSendTopologyConfiguration and using ISendEndpointProvider
        cfg.Send<CustomerCreatedV1>(e =>
        {
            // route by message type to binding fanout exchange (exchange to exchange binding)
            e.UseRoutingKeyFormatter(context => context.Message.GetType().Name.Underscore());
        });
        cfg.SetQueueArgument(
            "x-dead-letter-exchange",
            $"{nameof(CustomerCreatedV1).Underscore()}_dead_letter_exchange"
        );
        cfg.SetQueueArgument("x-dead-letter-routing-key", nameof(CustomerCreatedV1).Underscore());
    }
}
