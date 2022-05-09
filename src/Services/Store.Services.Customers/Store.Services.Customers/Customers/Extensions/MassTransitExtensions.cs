using Store.Services.Shared.Customers.Customers.Events.Integration;
using Humanizer;
using MassTransit;
using RabbitMQ.Client;

namespace Store.Services.Customers.Customers;

internal static class MassTransitExtensions
{
    internal static void AddCustomerPublishers(this IRabbitMqBusFactoryConfigurator cfg)
    {
        // Ref: https://wrapt.dev/blog/building-an-event-driven-dotnet-application-setting-up-masstransit-and-rabbitmq
        // Ref: https://masstransit-project.com/advanced/topology/publish.html
        // var publishTopology = new MessagePublishTopology<CustomerCreated>();
        // cfg.PublishTopology.AddMessagePublishTopology<CustomerCreated>(publishTopology);
        cfg.Publish<CustomerCreated>(x => { x.ExchangeType = ExchangeType.Topic; }); // primary exchange type

        // var sendPublishTopology = new MessageSendTopology<UserRegistered>();
        // sendPublishTopology.UseRoutingKeyFormatter(context => nameof(UserRegistered).Underscore());
        // cfg.SendTopology.AddMessageSendTopology<UserRegistered>(sendPublishTopology);

        // Ref: https://masstransit-project.com/advanced/topology/send.html
        cfg.Send<CustomerCreated>(x =>
        {
            x.UseRoutingKeyFormatter(context => nameof(CustomerCreated).Underscore());
        }); // route by provider
        cfg.Message<CustomerCreated>(x => x.SetEntityName(nameof(CustomerCreated).Underscore())); // name of the primary exchange
    }
}
