using MassTransit;

namespace Store.Services.Customers.RestockSubscriptions;

internal static class MassTransitExtensions
{
    internal static void AddRestockSubscriptionPublishers(this IRabbitMqBusFactoryConfigurator cfg)
    {
    }
}
