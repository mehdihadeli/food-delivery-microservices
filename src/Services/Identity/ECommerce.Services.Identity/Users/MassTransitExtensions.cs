using ECommerce.Services.Identity.Users.Features.UpdatingUserState.v1.Events.Integration;
using ECommerce.Services.Shared.Identity.Users.Events.v1.Integration;
using Humanizer;
using MassTransit;
using RabbitMQ.Client;

namespace ECommerce.Services.Identity.Users;

internal static class MassTransitExtensions
{
    internal static void AddUserPublishers(this IRabbitMqBusFactoryConfigurator cfg)
    {
        cfg.Message<UserRegisteredV1>(e =>
            e.SetEntityName($"{nameof(UserRegisteredV1).Underscore()}.input_exchange")); // name of the primary exchange
        cfg.Publish<UserRegisteredV1>(e => e.ExchangeType = ExchangeType.Direct); // primary exchange type
        cfg.Send<UserRegisteredV1>(e =>
        {
            // route by message type to binding fanout exchange (exchange to exchange binding)
            e.UseRoutingKeyFormatter(context =>
                context.Message.GetType().Name.Underscore());
        });

        cfg.Message<UserStateUpdated>(e =>
            e.SetEntityName($"{nameof(UserStateUpdated).Underscore()}.input_exchange")); // name of the primary exchange
        cfg.Publish<UserStateUpdated>(e => e.ExchangeType = ExchangeType.Direct); // primary exchange type
        cfg.Send<UserStateUpdated>(e =>
        {
            // route by message type to binding fanout exchange (exchange to exchange binding)
            e.UseRoutingKeyFormatter(context =>
                context.Message.GetType().Name.Underscore());
        });
    }
}
