using Humanizer;
using MassTransit;
using Store.Services.Customers.Customers.Features.CreatingCustomer.Events.Integration;
using Store.Services.Shared.Customers.Customers.Events.Integration;

namespace Store.Services.Customers.Customers;

internal static class MassTransitExtensions
{
    internal static void AddCustomerPublishers(this IRabbitMqBusFactoryConfigurator cfg)
    {
    }

    internal static void AddCustomerEndpoints(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
    {
        cfg.ReceiveEndpoint(nameof(CustomerCreated).Underscore() + ".customers", e =>
        {
            e.RethrowFaultedMessages();
            e.ConfigureConsumer<CustomerCreatedConsumer>(context);
        });
    }

    internal static void AddCustomerConsumers(this IBusRegistrationConfigurator cfg)
    {
        cfg.AddConsumer<CustomerCreatedConsumer>()
            .Endpoint(e => { e.ConcurrentMessageLimit = 1; });
    }
}
