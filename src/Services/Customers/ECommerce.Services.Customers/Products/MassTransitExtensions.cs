using ECommerce.Services.Customers.Products.Features.CreatingProduct.Events.Integration.External;
using ECommerce.Services.Customers.Products.Features.ReplenishingProductStock.Events.Integration.External;
using Humanizer;
using MassTransit;
using RabbitMQ.Client;
using ECommerce.Services.Shared.Catalogs.Products.Events.Integration;

namespace ECommerce.Services.Customers.Products;

internal static class MassTransitExtensions
{
    internal static void AddProductEndpoints(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
    {
        cfg.ReceiveEndpoint(nameof(ProductStockReplenished).Underscore(), re =>
        {
            // turns off default fanout settings
            re.ConfigureConsumeTopology = false;

            // a replicated queue to provide high availability and data safety. available in RMQ 3.8+
            re.SetQuorumQueue();

            re.Bind($"{nameof(ProductStockReplenished).Underscore()}.input_exchange", e =>
            {
                e.RoutingKey = nameof(ProductStockReplenished).Underscore();
                e.ExchangeType = ExchangeType.Direct;
            });

            // https://github.com/MassTransit/MassTransit/discussions/3117
            // https://masstransit-project.com/usage/configuration.html#receive-endpoints
            re.ConfigureConsumer<ProductStockReplenishedConsumer>(context);

            re.RethrowFaultedMessages();
        });

        cfg.ReceiveEndpoint(nameof(ProductCreated).Underscore(), re =>
        {
            // turns off default fanout settings
            re.ConfigureConsumeTopology = false;

            // a replicated queue to provide high availability and data safety. available in RMQ 3.8+
            re.SetQuorumQueue();

            re.Bind($"{nameof(ProductCreated).Underscore()}.input_exchange", e =>
            {
                e.RoutingKey = nameof(ProductCreated).Underscore();
                e.ExchangeType = ExchangeType.Direct;
            });

            // https://github.com/MassTransit/MassTransit/discussions/3117
            // https://masstransit-project.com/usage/configuration.html#receive-endpoints
            re.ConfigureConsumer<ProductCreatedConsumer>(context);

            re.RethrowFaultedMessages();
        });
    }
}
