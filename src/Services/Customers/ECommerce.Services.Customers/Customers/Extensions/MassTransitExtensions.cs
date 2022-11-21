using ECommerce.Services.Shared.Customers.Customers.Events.v1.Integration;
using Humanizer;
using MassTransit;
using RabbitMQ.Client;

namespace ECommerce.Services.Customers.Customers;

internal static class MassTransitExtensions
{
    internal static void AddCustomerPublishers(this IRabbitMqBusFactoryConfigurator cfg)
    {
        cfg.Message<CustomerCreatedV1>(e =>
            e.SetEntityName($"{nameof(CustomerCreatedV1).Underscore()}.input_exchange")); // name of the primary exchange
        cfg.Publish<CustomerCreatedV1>(e => e.ExchangeType = ExchangeType.Direct); // primary exchange type
        cfg.Send<CustomerCreatedV1>(e =>
        {
            // route by message type to binding fanout exchange (exchange to exchange binding)
            e.UseRoutingKeyFormatter(context =>
                context.Message.GetType().Name.Underscore());
        });
    }
}
