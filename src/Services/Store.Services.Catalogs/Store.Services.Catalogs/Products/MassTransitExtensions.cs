using Humanizer;
using MassTransit;
using RabbitMQ.Client;
using Store.Services.Shared.Catalogs.Products.Events.Integration;

namespace Store.Services.Catalogs.Products;

public static class MassTransitExtensions
{
    internal static void AddProductPublishers(this IRabbitMqBusFactoryConfigurator cfg)
    {
        cfg.Message<ProductCreated>(e =>
            e.SetEntityName($"{nameof(ProductCreated).Underscore()}.input_exchange")); // name of the primary exchange
        cfg.Publish<ProductCreated>(e => e.ExchangeType = ExchangeType.Direct); // primary exchange type
        cfg.Send<ProductCreated>(e =>
        {
            // route by message type to binding fanout exchange (exchange to exchange binding)
            e.UseRoutingKeyFormatter(context =>
                context.Message.GetType().Name.Underscore());
        });

        cfg.Message<ProductStockDebited>(e =>
            e.SetEntityName(
                $"{nameof(ProductStockDebited).Underscore()}.input_exchange")); // name of the primary exchange
        cfg.Publish<ProductStockDebited>(e => e.ExchangeType = ExchangeType.Direct); // primary exchange type
        cfg.Send<ProductStockDebited>(e =>
        {
            // route by message type to binding fanout exchange (exchange to exchange binding)
            e.UseRoutingKeyFormatter(context =>
                context.Message.GetType().Name.Underscore());
        });

        cfg.Message<ProductStockReplenished>(e =>
            e.SetEntityName(
                $"{nameof(ProductStockReplenished).Underscore()}.input_exchange")); // name of the primary exchange
        cfg.Publish<ProductStockReplenished>(e => e.ExchangeType = ExchangeType.Direct); // primary exchange type
        cfg.Send<ProductStockReplenished>(e =>
        {
            // route by message type to binding fanout exchange (exchange to exchange binding)
            e.UseRoutingKeyFormatter(context =>
                context.Message.GetType().Name.Underscore());
        });
    }
}
