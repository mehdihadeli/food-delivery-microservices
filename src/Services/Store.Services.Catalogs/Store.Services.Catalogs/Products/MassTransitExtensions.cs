using Store.Services.Shared.Catalogs.Products.Events.Integration;
using Humanizer;
using MassTransit;
using RabbitMQ.Client;

namespace Store.Services.Catalogs.Products;

public static class MassTransitExtensions
{
    internal static void AddProductPublishers(this IRabbitMqBusFactoryConfigurator cfg)
    {
        // Ref: https://wrapt.dev/blog/building-an-event-driven-dotnet-application-setting-up-masstransit-and-rabbitmq
        // Ref: https://masstransit-project.com/advanced/topology/publish.html
        cfg.Publish<ProductCreated>(x => { x.ExchangeType = ExchangeType.Topic; }); // primary exchange type

        // Ref: https://masstransit-project.com/advanced/topology/send.html
        cfg.Send<ProductCreated>(x =>
        {
            x.UseRoutingKeyFormatter(context => nameof(ProductCreated).Underscore()); // route by provider
        });
        cfg.Message<ProductCreated>(x =>
            x.SetEntityName(nameof(ProductCreated).Underscore())); // name of the primary exchange

        cfg.Publish<ProductStockReplenished>(x => { x.ExchangeType = ExchangeType.Topic; }); // primary exchange type
        cfg.Send<ProductStockReplenished>(x =>
        {
            x.UseRoutingKeyFormatter(context => nameof(ProductStockReplenished).Underscore()); // route by provider
        });
        cfg.Message<ProductStockReplenished>(x =>
            x.SetEntityName(nameof(ProductStockReplenished).Underscore())); // name of the primary exchange

        cfg.Publish<ProductStockDebited>(x => { x.ExchangeType = ExchangeType.Topic; }); // primary exchange type
        cfg.Send<ProductStockDebited>(x =>
        {
            x.UseRoutingKeyFormatter(context => nameof(ProductStockDebited).Underscore()); // route by provider
        });
        cfg.Message<ProductStockDebited>(x =>
            x.SetEntityName(nameof(ProductStockDebited).Underscore())); // name of the primary exchange
    }
}
