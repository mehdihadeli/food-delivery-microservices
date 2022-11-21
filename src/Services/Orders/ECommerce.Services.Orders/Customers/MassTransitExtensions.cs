using ECommerce.Services.Orders.Customers.Features.CreatingCustomer.v1.Events.External;
using ECommerce.Services.Shared.Customers.Customers.Events.v1.Integration;
using Humanizer;
using MassTransit;
using RabbitMQ.Client;

namespace ECommerce.Services.Orders.Customers;

internal static class MassTransitExtensions
{
    internal static void AddCustomerEndpoints(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
    {
        cfg.ReceiveEndpoint(nameof(CustomerCreatedV1).Underscore(), re =>
        {
            // turns off default fanout settings
            re.ConfigureConsumeTopology = false;

            // a replicated queue to provide high availability and data safety. available in RMQ 3.8+
            re.SetQuorumQueue();

            re.Bind($"{nameof(CustomerCreatedV1).Underscore()}.input_exchange", e =>
            {
                e.RoutingKey = nameof(CustomerCreatedV1).Underscore();
                e.ExchangeType = ExchangeType.Direct;
            });

            // https://github.com/MassTransit/MassTransit/discussions/3117
            // https://masstransit-project.com/usage/configuration.html#receive-endpoints
            re.ConfigureConsumer<CustomerCreatedConsumer>(context);

            re.RethrowFaultedMessages();
        });
    }
}
