using Humanizer;
using MassTransit;
using RabbitMQ.Client;
using Store.Services.Shared.Catalogs.Products.Events.Integration;

namespace Store.Services.Catalogs.Products;

public static class MassTransitExtensions
{
    internal static void AddProductPublishers(this IRabbitMqBusFactoryConfigurator cfg)
    {
    }
}
