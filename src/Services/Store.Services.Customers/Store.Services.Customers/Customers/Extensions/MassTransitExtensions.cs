using Humanizer;
using MassTransit;
using RabbitMQ.Client;
using Store.Services.Shared.Customers.Customers.Events.Integration;

namespace Store.Services.Customers.Customers;

internal static class MassTransitExtensions
{
    internal static void AddCustomerPublishers(this IRabbitMqBusFactoryConfigurator cfg)
    {
    }
}
