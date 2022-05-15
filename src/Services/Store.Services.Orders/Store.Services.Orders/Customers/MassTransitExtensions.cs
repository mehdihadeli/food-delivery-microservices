using Humanizer;
using MassTransit;
using RabbitMQ.Client;
using Store.Services.Orders.Customers.Features.CreatingCustomer.Events.External;
using Store.Services.Shared.Customers.Customers.Events.Integration;

namespace Store.Services.Orders.Customers;

internal static class MassTransitExtensions
{
    internal static void AddCustomerEndpoints(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
    {
        cfg.ReceiveEndpoint(nameof(CustomerCreated).Underscore(), re =>
        {
            // turns off default fanout settings
            re.ConfigureConsumeTopology = false;

            // a replicated queue to provide high availability and data safety. available in RMQ 3.8+
            re.SetQuorumQueue();

            re.Bind($"{nameof(CustomerCreated).Underscore()}.input_exchange", e =>
            {
                e.RoutingKey = nameof(CustomerCreated).Underscore();
                e.ExchangeType = ExchangeType.Direct;
            });

            // https://github.com/MassTransit/MassTransit/discussions/3117
            // https://masstransit-project.com/usage/configuration.html#receive-endpoints
            re.ConfigureConsumer<CustomerCreatedConsumer>(context);

            re.RethrowFaultedMessages();
        });
    }
}
