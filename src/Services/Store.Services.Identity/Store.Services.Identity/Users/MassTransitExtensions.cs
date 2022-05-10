using Humanizer;
using MassTransit;
using RabbitMQ.Client;
using Store.Services.Shared.Identity.Users.Events.Integration;

namespace Store.Services.Identity.Users;

internal static class MassTransitExtensions
{
    internal static void AddUserPublishers(this IRabbitMqBusFactoryConfigurator cfg)
    {
    }
}
