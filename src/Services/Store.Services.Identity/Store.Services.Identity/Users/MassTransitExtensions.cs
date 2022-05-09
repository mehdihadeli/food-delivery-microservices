using Store.Services.Shared.Identity.Users.Events.Integration;
using Humanizer;
using MassTransit;
using RabbitMQ.Client;

namespace Store.Services.Identity.Users;

internal static class MassTransitExtensions
{
    internal static void AddUserPublishers(this IRabbitMqBusFactoryConfigurator cfg)
    {
        // Ref: https://wrapt.dev/blog/building-an-event-driven-dotnet-application-setting-up-masstransit-and-rabbitmq
        // Ref: https://masstransit-project.com/advanced/topology/publish.html
        // var publishTopology = new MessagePublishTopology<CustomerCreated>();
        // cfg.PublishTopology.AddMessagePublishTopology<CustomerCreated>(publishTopology);
        cfg.Publish<UserRegistered>(x => { x.ExchangeType = ExchangeType.Topic; }); // primary exchange type

        // var sendPublishTopology = new MessageSendTopology<UserRegistered>();
        // sendPublishTopology.UseRoutingKeyFormatter(context => nameof(UserRegistered).Underscore());
        // cfg.SendTopology.AddMessageSendTopology<UserRegistered>(sendPublishTopology);

        // Ref: https://masstransit-project.com/advanced/topology/send.html
        cfg.Send<UserRegistered>(x =>
        {
            x.UseRoutingKeyFormatter(context => nameof(UserRegistered).Underscore());
        }); // route by provider
        cfg.Message<UserRegistered>(x =>
            x.SetEntityName(nameof(UserRegistered).Underscore())); // name of the primary exchange


        cfg.Publish<UserStateUpdated>(x => { x.ExchangeType = ExchangeType.Topic; }); // primary exchange type
        cfg.Send<UserStateUpdated>(x =>
        {
            x.UseRoutingKeyFormatter(context => nameof(UserStateUpdated).Underscore());
        }); // route by provider
        cfg.Message<UserStateUpdated>(x =>
            x.SetEntityName(nameof(UserStateUpdated).Underscore())); // name of the primary exchange
    }
}
