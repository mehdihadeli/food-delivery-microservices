using Store.Services.Customers.Identity.Features.RegisteringUser.Events.External;
using Store.Services.Shared.Identity.Users.Events.Integration;
using Humanizer;
using MassTransit;
using RabbitMQ.Client;

namespace Store.Services.Customers.Identity;

internal static class MassTransitExtensions
{
    internal static void AddIdentityConsumers(this IBusRegistrationConfigurator cfg)
    {
        cfg.AddConsumer<UserRegisteredConsumer>()
            .Endpoint(e => { e.ConcurrentMessageLimit = 1; });
    }

    internal static void AddIdentityEndpoints(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
    {
        // Ref: https://wrapt.dev/blog/building-an-event-driven-dotnet-application-setting-up-masstransit-and-rabbitmq
        // Ref: https://masstransit-project.com/advanced/topology/consume.html
        // cfg.ConsumeTopology.AddMessageConsumeTopology(new MessageConsumeTopology<>());
        cfg.ReceiveEndpoint(
            queueName: nameof(UserRegistered).Underscore(),
            receiveEndpointConfigurator =>
            {
                // turns off default fanout settings
                receiveEndpointConfigurator.ConfigureConsumeTopology = false;

                // a replicated queue to provide high availability and data safety. available in RMQ 3.8+
                receiveEndpointConfigurator.SetQuorumQueue();

                receiveEndpointConfigurator.ConfigureConsumer<UserRegisteredConsumer>(context);
                receiveEndpointConfigurator.Bind(
                    exchangeName: nameof(UserRegistered).Underscore(),
                    x =>
                    {
                        x.ExchangeType = ExchangeType.Topic;
                        x.RoutingKey = nameof(UserRegistered).Underscore();
                    });
            });
    }
}
