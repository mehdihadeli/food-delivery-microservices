using FoodDelivery.Services.Shared.Catalogs.Products.Events.Integration.v1;
using Humanizer;
using MassTransit;
using RabbitMQ.Client;

namespace FoodDelivery.Services.Shared.Customers.Products;

public static class MassTransitExtensions
{
    public static void ConfigureProductsConsumeMessagesTopology(
        this IRabbitMqBusFactoryConfigurator cfg,
        IBusRegistrationContext context
    )
    {
        // Just for test purpose of `CustomersService` separately
        // configuration for MessagePublishTopologyConfiguration and using IPublishEndpoint
        cfg.Publish<ProductStockReplenishedV1>(e =>
        {
            // setup primary exchange
            e.Durable = true;
            e.ExchangeType = ExchangeType.Direct;
        });

        // https://github.com/MassTransit/MassTransit/blob/eb3c9ee1007cea313deb39dc7c4eb796b7e61579/src/Transports/MassTransit.RabbitMqTransport/RabbitMqTransport/Configuration/RabbitMqReceiveEndpointBuilder.cs#L70
        // https://spring.io/blog/2011/04/01/routing-topologies-for-performance-and-scalability-with-rabbitmq

        // https://masstransit.io/documentation/transports/rabbitmq
        // This `queueName` creates an `intermediary exchange` (default is Fanout, but we can change it with re.ExchangeType) with the same queue named which bound to this exchange
        cfg.ReceiveEndpoint(
            nameof(ProductStockReplenishedV1).Underscore(),
            re =>
            {
                re.Durable = true;

                // set intermediate exchange type
                // intermediate exchange name will be the same as queue name
                re.ExchangeType = ExchangeType.Fanout;

                // a replicated queue to provide high availability and data safety. available in RMQ 3.8+
                re.SetQuorumQueue();

                // with setting `ConfigureConsumeTopology` to `false`, we should create `primary exchange` and its bounded exchange manually with using `re.Bind` otherwise with `ConfigureConsumeTopology=true` it get publish topology for message type `T` with `_publishTopology.GetMessageTopology<T>()` and use its ExchangeType and ExchangeName based ofo default EntityFormatter
                // indicate whether the topic or exchange for the message type should be created and subscribed to the queue when consumed on a reception endpoint.
                re.ConfigureConsumeTopology = false;

                // https://spring.io/blog/2011/04/01/routing-topologies-for-performance-and-scalability-with-rabbitmq
                // masstransit uses `wire-tapping` pattern for defining exchanges. Primary exchange will send the message to intermediary fanout exchange
                // setup primary exchange and its type from message type and receive-endpoint formatter
                re.Bind<ProductStockReplenishedV1>(e =>
                {
                    e.RoutingKey = nameof(ProductStockReplenishedV1).Underscore();
                    e.ExchangeType = ExchangeType.Direct;
                });

                // https://github.com/MassTransit/MassTransit/discussions/3117
                // https://masstransit-project.com/usage/configuration.html#receive-endpoints
                re.ConfigureConsumers(context);

                re.RethrowFaultedMessages();
            }
        );

        // Just for test purpose of `CustomersService` separately
        // configuration for MessagePublishTopologyConfiguration and using IPublishEndpoint
        cfg.Publish<ProductCreatedV1>(e =>
        {
            // setup primary exchange
            e.Durable = true;
            e.ExchangeType = ExchangeType.Direct;
        });
        // https://masstransit.io/documentation/transports/rabbitmq
        // This `queueName` creates an `intermediary exchange` (default is Fanout, but we can change it with re.ExchangeType) with the same queue named which bound to this exchange
        cfg.ReceiveEndpoint(
            nameof(ProductCreatedV1).Underscore(),
            re =>
            {
                re.Durable = true;

                // set intermediate exchange type
                // intermediate exchange name will be the same as queue name
                re.ExchangeType = ExchangeType.Fanout;

                // a replicated queue to provide high availability and data safety. available in RMQ 3.8+
                re.SetQuorumQueue();

                // with setting `ConfigureConsumeTopology` to `false`, we should create `primary exchange` and its bounded exchange manually with using `re.Bind` otherwise with `ConfigureConsumeTopology=true` it get publish topology for message type `T` with `_publishTopology.GetMessageTopology<T>()` and use its ExchangeType and ExchangeName based ofo default EntityFormatter
                re.ConfigureConsumeTopology = false;

                // https://spring.io/blog/2011/04/01/routing-topologies-for-performance-and-scalability-with-rabbitmq
                // masstransit uses `wire-tapping` pattern for defining exchanges. Primary exchange will send the message to intermediary fanout exchange
                // setup primary exchange and its type
                re.Bind<ProductCreatedV1>(e =>
                {
                    e.RoutingKey = nameof(ProductCreatedV1).Underscore();
                    e.ExchangeType = ExchangeType.Direct;
                });

                // https://github.com/MassTransit/MassTransit/discussions/3117
                // https://masstransit-project.com/usage/configuration.html#receive-endpoints
                re.ConfigureConsumers(context);

                re.RethrowFaultedMessages();
            }
        );
    }
}
