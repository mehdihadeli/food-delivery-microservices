using BuildingBlocks.Abstractions.Events;
using FoodDelivery.Services.Customers.Users.Features.RegisteringUser.v1.Events.Integration.External;
using FoodDelivery.Services.Shared.Identity.Users.Events.V1.Integration;
using Humanizer;
using MassTransit;
using RabbitMQ.Client;

namespace FoodDelivery.Services.Customers.Users;

internal static class MassTransitExtensions
{
    internal static void ConfigureUsersMessagesTopology(
        this IRabbitMqBusFactoryConfigurator cfg,
        IBusRegistrationContext context
    )
    {
        // we configured some shared settings for all publish message in masstransit publish topologies

        // // https://github.com/MassTransit/MassTransit/blob/eb3c9ee1007cea313deb39dc7c4eb796b7e61579/src/Transports/MassTransit.RabbitMqTransport/RabbitMqTransport/Configuration/RabbitMqReceiveEndpointBuilder.cs#L70
        // // https://spring.io/blog/2011/04/01/routing-topologies-for-performance-and-scalability-with-rabbitmq
        //
        // // https://masstransit.io/documentation/transports/rabbitmq
        // // This `queueName` creates an intermediary exchange (default is Fanout, but we can change it with re.ExchangeType) with the same queue named which bound to this exchange
        // cfg.ReceiveEndpoint(
        //     queueName: nameof(UserRegisteredV1).Underscore(),
        //     re =>
        //     {
        //         re.Durable = true;
        //
        //         // set intermediate exchange type
        //         // intermediate exchange name will be the same as queue name
        //         re.ExchangeType = ExchangeType.Fanout;
        //
        //         // a replicated queue to provide high availability and data safety. available in RMQ 3.8+
        //         re.SetQuorumQueue();
        //
        //         // with setting `ConfigureConsumeTopology` to `false`, we should create `primary exchange` and its bounded exchange manually with using `re.Bind` otherwise with `ConfigureConsumeTopology=true` it get publish topology for message type `T` with `_publishTopology.GetMessageTopology<T>()` and use its ExchangeType and ExchangeName based ofo default EntityFormatter
        //         // indicate whether the topic or exchange for the message type should be created and subscribed to the queue when consumed on a reception endpoint.
        //         re.ConfigureConsumeTopology = true;
        //
        //         // // https://spring.io/blog/2011/04/01/routing-topologies-for-performance-and-scalability-with-rabbitmq
        //         // // masstransit uses `wire-tapping` pattern for defining exchanges. Primary exchange will send the message to intermediary fanout exchange
        //         // // setup primary exchange and its type from message type and receive-endpoint formatter
        //         // re.Bind<IEventEnvelope<UserRegisteredV1>>(e =>
        //         // {
        //         //     e.RoutingKey = nameof(UserRegisteredV1).Underscore();
        //         //     e.ExchangeType = ExchangeType.Direct;
        //         // });
        //
        //         // https://github.com/MassTransit/MassTransit/discussions/3117
        //         // https://masstransit-project.com/usage/configuration.html#receive-endpoints
        //         re.ConfigureConsumer<UserRegisteredConsumer>(context);
        //
        //         re.RethrowFaultedMessages();
        //     }
        // );
    }
}
