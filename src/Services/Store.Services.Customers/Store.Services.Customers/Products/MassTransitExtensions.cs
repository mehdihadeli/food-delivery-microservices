using Store.Services.Customers.Products.Features.ReplenishingProductStock.Events.External;
using Store.Services.Shared.Catalogs.Products.Events.Integration;
using Humanizer;
using MassTransit;
using RabbitMQ.Client;

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
        // Ref: https://wrapt.dev/blog/building-an-event-driven-dotnet-application-setting-up-masstransit-and-rabbitmq
        // Ref: https://masstransit-project.com/advanced/topology/consume.html
        // cfg.ConsumeTopology.AddMessageConsumeTopology(new MessageConsumeTopology<>());
        cfg.ReceiveEndpoint(
            queueName: nameof(ProductStockReplenished).Underscore(),
            receiveEndpointConfigurator =>
            {
                // turns off default fanout settings
                receiveEndpointConfigurator.ConfigureConsumeTopology = false;

                // a replicated queue to provide high availability and data safety. available in RMQ 3.8+
                receiveEndpointConfigurator.SetQuorumQueue();

                receiveEndpointConfigurator.ConfigureConsumer<ProductStockReplenishedConsumer>(context);
                receiveEndpointConfigurator.Bind(exchangeName: nameof(ProductStockReplenished).Underscore(), x =>
                {
                    x.ExchangeType = ExchangeType.Topic;
                    x.RoutingKey = nameof(ProductStockReplenished).Underscore();
                });
            });
    }
}
