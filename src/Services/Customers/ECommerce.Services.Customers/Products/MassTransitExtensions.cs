using ECommerce.Services.Customers.Products.Features.CreatingProduct.v1.Events.Integration.External;
using ECommerce.Services.Customers.Products.Features.ReplenishingProductStock.v1.Events.Integration.External;
using ECommerce.Services.Shared.Catalogs.Products.Events.v1.Integration;
using Humanizer;
using MassTransit;
using RabbitMQ.Client;

namespace ECommerce.Services.Customers.Products;

internal static class MassTransitExtensions
{
    internal static void AddProductEndpoints(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
    {
        cfg.ReceiveEndpoint(nameof(ProductStockReplenishedV1).Underscore(), re =>
        {
            // turns off default fanout settings
            re.ConfigureConsumeTopology = true;

            // a replicated queue to provide high availability and data safety. available in RMQ 3.8+
            re.SetQuorumQueue();

            re.Bind($"{nameof(ProductStockReplenishedV1).Underscore()}.input_exchange", e =>
            {
                e.RoutingKey = nameof(ProductStockReplenishedV1).Underscore();
                e.ExchangeType = ExchangeType.Direct;
            });

            // https://github.com/MassTransit/MassTransit/discussions/3117
            // https://masstransit-project.com/usage/configuration.html#receive-endpoints
            re.ConfigureConsumer<ProductStockReplenishedConsumer>(context);

            re.RethrowFaultedMessages();
        });

        cfg.ReceiveEndpoint(nameof(ProductCreatedV1).Underscore(), re =>
        {
            // turns off default fanout settings
            re.ConfigureConsumeTopology = true;

            // a replicated queue to provide high availability and data safety. available in RMQ 3.8+
            re.SetQuorumQueue();

            re.Bind($"{nameof(ProductCreatedV1).Underscore()}.input_exchange", e =>
            {
                e.RoutingKey = nameof(ProductCreatedV1).Underscore();
                e.ExchangeType = ExchangeType.Direct;
            });

            // https://github.com/MassTransit/MassTransit/discussions/3117
            // https://masstransit-project.com/usage/configuration.html#receive-endpoints
            re.ConfigureConsumer<ProductCreatedConsumer>(context);

            re.RethrowFaultedMessages();
        });
    }
}
