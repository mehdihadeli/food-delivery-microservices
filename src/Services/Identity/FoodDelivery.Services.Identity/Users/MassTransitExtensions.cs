using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Integration.MassTransit;
using FoodDelivery.Services.Identity.Users.Features.UpdatingUserState.v1.Events.Integration;
using FoodDelivery.Services.Shared.Identity.Users.Events.V1.Integration;
using Humanizer;
using MassTransit;

namespace FoodDelivery.Services.Identity.Users;

internal static class MassTransitExtensions
{
    internal static void ConfigureUserMessagesTopology(this IRabbitMqBusFactoryConfigurator cfg)
    {
        // https://masstransit.io/documentation/transports/rabbitmq
        cfg.Message<IEventEnvelope<UserRegisteredV1>>(e =>
        {
            // https://masstransit.io/documentation/configuration/topology/message
            // Name of the `primary exchange` for type based message publishing and sending
            // e.SetEntityName($"{nameof(UserRegisteredV1).Underscore()}{MessagingConstants.PrimaryExchangePostfix}");
            e.SetEntityNameFormatter(new CustomEntityNameFormatter<IEventEnvelope<UserRegisteredV1>>());
        });

        // configuration for MessagePublishTopologyConfiguration and using IPublishEndpoint
        cfg.Publish<IEventEnvelope<UserRegisteredV1>>(e =>
        {
            // we configured some shared settings for all publish message in masstransit publish topologies

            // // setup primary exchange
            // e.Durable = true;
            // e.ExchangeType = ExchangeType.Direct;
        });

        // configuration for MessageSendTopologyConfiguration and using ISendEndpointProvider
        cfg.Send<IEventEnvelope<UserRegisteredV1>>(e =>
        {
            // route by message type to binding fanout exchange (exchange to exchange binding)
            e.UseRoutingKeyFormatter(context => context.Message.GetType().Name.Underscore());
        });

        cfg.Message<IEventEnvelope<UserStateUpdated>>(e =>
        {
            // https://masstransit.io/documentation/configuration/topology/message
            // Name of the `primary exchange` for type based message publishing and sending
            // e.SetEntityName($"{nameof(UserStateUpdated).Underscore()}{MessagingConstants.PrimaryExchangePostfix}");
            e.SetEntityNameFormatter(new CustomEntityNameFormatter<IEventEnvelope<UserStateUpdated>>());
        });

        cfg.Publish<IEventEnvelope<UserStateUpdated>>(e =>
        {
            // we configured some shared settings for all publish message in masstransit publish topologies

            // // setup primary exchange
            // e.Durable = true;
            // e.ExchangeType = ExchangeType.Direct;
        });

        // configuration for MessageSendTopologyConfiguration and using ISendEndpointProvider
        cfg.Send<IEventEnvelope<UserStateUpdated>>(e =>
        {
            // route by message type to binding fanout exchange (exchange to exchange binding)
            e.UseRoutingKeyFormatter(context => context.Message.GetType().Name.Underscore());
        });
    }
}
