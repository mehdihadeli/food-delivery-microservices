using ECommerce.Services.Shared.Catalogs.Products.Events.v1.Integration;
using Humanizer;
using MassTransit;
using RabbitMQ.Client;

namespace ECommerce.Services.Catalogs.Products;

public static class MassTransitExtensions
{
    internal static void AddProductPublishers(this IRabbitMqBusFactoryConfigurator cfg)
    {
        cfg.Message<ProductCreatedV1>(e =>
            e.SetEntityName($"{nameof(ProductCreatedV1).Underscore()}.input_exchange")); // name of the primary exchange
        cfg.Publish<ProductCreatedV1>(e => e.ExchangeType = ExchangeType.Direct); // primary exchange type
        cfg.Send<ProductCreatedV1>(e =>
        {
            // route by message type to binding fanout exchange (exchange to exchange binding)
            e.UseRoutingKeyFormatter(context =>
                context.Message.GetType().Name.Underscore());
        });

        cfg.Message<ProductStockDebitedV1>(e =>
            e.SetEntityName(
                $"{nameof(ProductStockDebitedV1).Underscore()}.input_exchange")); // name of the primary exchange
        cfg.Publish<ProductStockDebitedV1>(e => e.ExchangeType = ExchangeType.Direct); // primary exchange type
        cfg.Send<ProductStockDebitedV1>(e =>
        {
            // route by message type to binding fanout exchange (exchange to exchange binding)
            e.UseRoutingKeyFormatter(context =>
                context.Message.GetType().Name.Underscore());
        });

        cfg.Message<ProductStockReplenishedV1>(e =>
            e.SetEntityName(
                $"{nameof(ProductStockReplenishedV1).Underscore()}.input_exchange")); // name of the primary exchange
        cfg.Publish<ProductStockReplenishedV1>(e => e.ExchangeType = ExchangeType.Direct); // primary exchange type
        cfg.Send<ProductStockReplenishedV1>(e =>
        {
            // route by message type to binding fanout exchange (exchange to exchange binding)
            e.UseRoutingKeyFormatter(context =>
                context.Message.GetType().Name.Underscore());
        });
    }
}
