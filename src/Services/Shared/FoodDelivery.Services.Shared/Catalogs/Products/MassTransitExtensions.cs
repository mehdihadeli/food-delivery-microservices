using BuildingBlocks.Integration.MassTransit;
using FoodDelivery.Services.Shared.Catalogs.Products.Events.Integration.v1;
using Humanizer;
using MassTransit;
using RabbitMQ.Client;

namespace FoodDelivery.Services.Shared.Catalogs.Products;

public static class MassTransitExtensions
{
    public static void ConfigureProductPublishMessagesTopology(this IRabbitMqBusFactoryConfigurator cfg)
    {
        // https://masstransit.io/documentation/transports/rabbitmq
        cfg.Message<ProductCreatedV1>(e =>
        {
            // https://masstransit.io/documentation/configuration/topology/message
            // Name of the `primary exchange` for type based message publishing and sending
            // e.SetEntityName($"{nameof(ProductCreatedV1).Underscore()}{MessagingConstants.PrimaryExchangePostfix}");
            e.SetEntityNameFormatter(new CustomEntityNameFormatter<ProductCreatedV1>());
        });

        // configuration for MessagePublishTopologyConfiguration and using IPublishEndpoint
        cfg.Publish<ProductCreatedV1>(e =>
        {
            // we configured some shared settings for all publish message in masstransit publish topologies
            // setup primary exchange
            e.Durable = true;
            e.ExchangeType = ExchangeType.Direct;
        });

        cfg.Send<ProductCreatedV1>(e =>
        {
            // route by message type to binding fanout exchange (exchange to exchange binding)
            e.UseRoutingKeyFormatter(context => context.Message.GetType().Name.Underscore());
        });

        // https://masstransit.io/documentation/transports/rabbitmq
        cfg.Message<ProductStockDebitedV1>(e =>
        {
            // https://masstransit.io/documentation/configuration/topology/message
            // Name of the `primary exchange` for type based message publishing and sending
            // e.SetEntityName($"{nameof(ProductStockDebitedV1).Underscore()}{MessagingConstants.PrimaryExchangePostfix}");
            e.SetEntityNameFormatter(new CustomEntityNameFormatter<ProductStockDebitedV1>());
        });

        // configuration for MessagePublishTopologyConfiguration and using IPublishEndpoint
        cfg.Publish<ProductStockDebitedV1>(e =>
        {
            // setup primary exchange
            e.Durable = true;
            e.ExchangeType = ExchangeType.Direct;
        });

        cfg.Send<ProductStockDebitedV1>(e =>
        {
            // route by message type to binding fanout exchange (exchange to exchange binding)
            e.UseRoutingKeyFormatter(context => context.Message.GetType().Name.Underscore());
        });

        // https://masstransit.io/documentation/transports/rabbitmq
        cfg.Message<ProductStockReplenishedV1>(e =>
        {
            // https://masstransit.io/documentation/configuration/topology/message
            // Name of the `primary exchange` for type based message publishing and sending
            // e.SetEntityName($"{nameof(ProductStockDebitedV1).Underscore()}{MessagingConstants.PrimaryExchangePostfix}");
            e.SetEntityNameFormatter(new CustomEntityNameFormatter<ProductStockReplenishedV1>());
        });

        cfg.Publish<ProductStockReplenishedV1>(e =>
        {
            // setup primary exchange
            e.Durable = true;
            e.ExchangeType = ExchangeType.Direct;
        });

        cfg.Send<ProductStockReplenishedV1>(e =>
        {
            // route by message type to binding fanout exchange (exchange to exchange binding)
            e.UseRoutingKeyFormatter(context => context.Message.GetType().Name.Underscore());
        });
    }
}
