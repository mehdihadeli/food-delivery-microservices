using Humanizer;
using MassTransit;
using RabbitMQ.Client;
using Store.Services.Customers.Products.Features.ReplenishingProductStock.Events.External;
using Store.Services.Shared.Catalogs.Products.Events.Integration;

namespace Store.Services.Customers.Products;

internal static class MassTransitExtensions
{
    internal static void AddProductConsumers(this IBusRegistrationConfigurator cfg)
    {
        cfg.AddConsumer<ProductStockReplenishedConsumer>()
            .Endpoint(e => { e.ConcurrentMessageLimit = 1; });
    }

    internal static void AddProductEndpoints(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
    {
        cfg.ReceiveEndpoint(nameof(ProductStockReplenished).Underscore() + ".customers", e =>
        {
            e.RethrowFaultedMessages();
            e.ConfigureConsumer<ProductStockReplenishedConsumer>(context);
        });
    }
}
