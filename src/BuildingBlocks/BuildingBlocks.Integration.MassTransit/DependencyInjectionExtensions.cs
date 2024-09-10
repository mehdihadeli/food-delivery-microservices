using System.Reflection;
using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Exception.Types;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Reflection;
using BuildingBlocks.Core.Reflection.Extensions;
using BuildingBlocks.Core.Web.Extensions;
using Humanizer;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using IExternalEventBus = BuildingBlocks.Abstractions.Messaging.IExternalEventBus;

namespace BuildingBlocks.Integration.MassTransit;

public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Add Masstransit.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configureMessagesTopologies">All configurations related to message topology, publish configuration and recive endpoint configurations.</param>
    /// <param name="configureBusRegistration"></param>
    /// <param name="configureMessagingOptions"></param>
    /// <param name="scanAssemblies"></param>
    /// <returns></returns>
    public static WebApplicationBuilder AddCustomMassTransit(
        this WebApplicationBuilder builder,
        Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator>? configureMessagesTopologies = null,
        Action<IBusRegistrationConfigurator>? configureBusRegistration = null,
        Action<MessagingOptions>? configureMessagingOptions = null,
        params Assembly[] scanAssemblies
    )
    {
        builder.Services.AddValidationOptions(configureMessagingOptions);
        var messagingOptions = builder.Configuration.BindOptions(configureMessagingOptions);

        // - should be placed out of action delegate for getting correct calling assembly
        // - Assemblies are lazy loaded so using AppDomain.GetAssemblies is not reliable (it is possible to get ReflectionTypeLoadException, because some dependent type assembly are lazy and not loaded yet), so we use `GetAllReferencedAssemblies` and it
        // loads all referenced assemblies explicitly.
        var assemblies =
            scanAssemblies.Length != 0
                ? scanAssemblies
                : ReflectionUtilities.GetReferencedAssemblies(Assembly.GetCallingAssembly()).ToArray();

        if (!builder.Environment.IsTest())
        {
            builder.Services.AddMassTransit(ConfiguratorAction);
        }
        else
        {
            builder.Services.AddMassTransitTestHarness(ConfiguratorAction);
        }

        void ConfiguratorAction(IBusRegistrationConfigurator busRegistrationConfigurator)
        {
            // https://masstransit.io/documentation/configuration#health-check-options
            busRegistrationConfigurator.ConfigureHealthCheckOptions(options =>
            {
                options.Name = "masstransit";
                options.MinimalFailureStatus = HealthStatus.Unhealthy;
                options.Tags.Add("health");
            });

            configureBusRegistration?.Invoke(busRegistrationConfigurator);

            // https://masstransit-project.com/usage/configuration.html#receive-endpoints
            busRegistrationConfigurator.AddConsumers(assemblies);

            // https://masstransit.io/documentation/configuration#endpoint-name-formatters
            // uses by `ConfigureEndpoints` for naming `queues` and `receive endpoint` names
            busRegistrationConfigurator.SetEndpointNameFormatter(new SnakeCaseEndpointNameFormatter(false));

            // Ref: https://masstransit-project.com/advanced/topology/rabbitmq.html
            // https://masstransit-project.com/understand/publishing.html
            // https://spring.io/blog/2011/04/01/routing-topologies-for-performance-and-scalability-with-rabbitmq/
            // https://jstobigdata.com/rabbitmq/exchange-to-exchange-binding-in-rabbitmq/
            // https://masstransit-project.com/understand/under-the-hood.html
            // https://masstransit-project.com/usage/producers.html
            // https://masstransit-project.com/usage/consumers.html
            // https://masstransit-project.com/usage/messages.html
            // https://wrapt.dev/blog/building-an-event-driven-dotnet-application-setting-up-masstransit-and-rabbitmq
            busRegistrationConfigurator.UsingRabbitMq(
                (context, cfg) =>
                {
                    cfg.UseConsumeFilter(typeof(CorrelationConsumeFilter<>), context);

                    // https: // github.com/MassTransit/MassTransit/issues/2018
                    // https://github.com/MassTransit/MassTransit/issues/4831
                    cfg.Publish<IIntegrationEvent>(p => p.Exclude = true);
                    cfg.Publish<IntegrationEvent>(p => p.Exclude = true);
                    cfg.Publish<IMessage>(p => p.Exclude = true);
                    cfg.Publish<Message>(p => p.Exclude = true);
                    cfg.Publish<ITxRequest>(p => p.Exclude = true);
                    cfg.Publish<IEventEnvelope>(p => p.Exclude = true);

                    if (messagingOptions.AutoConfigEndpoints)
                    {
                        // https://masstransit.io/documentation/configuration#consumer-registration
                        // https://masstransit.io/documentation/configuration#configure-endpoints
                        // MassTransit is able to configure receive endpoints for all registered consumer types. Receive endpoint names are generated using an endpoint name formatter
                        cfg.ConfigureEndpoints(context);
                    }

                    var rabbitMqOptions = builder.Configuration.BindOptions<RabbitMqOptions>();

                    cfg.Host(
                        rabbitMqOptions.Host,
                        rabbitMqOptions.Port,
                        "/",
                        hostConfigurator =>
                        {
                            hostConfigurator.PublisherConfirmation = true;
                            hostConfigurator.Username(rabbitMqOptions.UserName);
                            hostConfigurator.Password(rabbitMqOptions.Password);
                        }
                    );

                    // for setting exchange name for message type as default. masstransit by default uses fully message type name for primary exchange name.
                    // https://masstransit.io/documentation/configuration/topology/message
                    cfg.MessageTopology.SetEntityNameFormatter(new CustomEntityNameFormatter());

                    ApplyMessagesPublishTopology(cfg.PublishTopology, assemblies);
                    ApplyMessagesConsumeTopology(cfg, context, assemblies);
                    ApplyMessagesSendTopology(cfg.SendTopology, assemblies);

                    configureMessagesTopologies?.Invoke(context, cfg);

                    // https://masstransit-project.com/usage/exceptions.html#retry
                    // https://markgossa.com/2022/06/masstransit-exponential-back-off.html
                    cfg.UseMessageRetry(r => AddRetryConfiguration(r));
                }
            );
        }

        builder
            .Services.AddOptions<MassTransitHostOptions>()
            .Configure(options =>
            {
                options.WaitUntilStarted = true;
                options.StartTimeout = TimeSpan.FromSeconds(30);
                options.StopTimeout = TimeSpan.FromSeconds(60);
            });

        builder
            .Services.AddOptions<HostOptions>()
            .Configure(options =>
            {
                options.StartupTimeout = TimeSpan.FromSeconds(60);
                options.ShutdownTimeout = TimeSpan.FromSeconds(60);
            });

        builder.Services.AddTransient<IExternalEventBus, MassTransitBus>();
        builder.Services.AddTransient<IBusDirectPublisher, MasstransitDirectPublisher>();

        return builder;
    }

    private static void ApplyMessagesSendTopology(
        IRabbitMqSendTopologyConfigurator sendTopology,
        Assembly[] assemblies
    ) { }

    private static void ApplyMessagesConsumeTopology(
        IRabbitMqBusFactoryConfigurator rabbitMqBusFactoryConfigurator,
        IBusRegistrationContext context,
        Assembly[] assemblies
    )
    {
        var consumeTopology = rabbitMqBusFactoryConfigurator.ConsumeTopology;

        var messageTypes = ReflectionUtilities
            .GetAllTypesImplementingInterface<IIntegrationEvent>(assemblies)
            .Where(x => !x.IsGenericType);

        foreach (var messageType in messageTypes)
        {
            var eventEnvelopeInterfaceMessageType = typeof(IEventEnvelope<>).MakeGenericType(messageType);
            var eventEnvelopeInterfaceConfigurator = consumeTopology.GetMessageTopology(
                eventEnvelopeInterfaceMessageType
            );

            // indicate whether the topic or exchange for the message type should be created and subscribed to the queue when consumed on a reception endpoint.
            // // with setting `ConfigureConsumeTopology` to `false`, we should create `primary exchange` and its bounded exchange manually with using `re.Bind` otherwise with `ConfigureConsumeTopology=true` it get publish topology for message type `T` with `_publishTopology.GetMessageTopology<T>()` and use its ExchangeType and ExchangeName based ofo default EntityFormatter
            eventEnvelopeInterfaceConfigurator.ConfigureConsumeTopology = true;

            // none event-envelope message types
            var messageConfigurator = consumeTopology.GetMessageTopology(messageType);
            messageConfigurator.ConfigureConsumeTopology = true;

            var eventEnvelopeConsumerInterface = typeof(IConsumer<>).MakeGenericType(eventEnvelopeInterfaceMessageType);
            var envelopeConsumerConcretedTypes = eventEnvelopeConsumerInterface
                .GetAllTypesImplementingInterface(assemblies)
                .Where(x => !x.FullName!.Contains(nameof(MassTransit)));

            var consumerType = envelopeConsumerConcretedTypes.SingleOrDefault();

            if (consumerType is null)
            {
                var messageTypeConsumerInterface = typeof(IConsumer<>).MakeGenericType(messageType);
                var messageTypeConsumerConcretedTypes = messageTypeConsumerInterface
                    .GetAllTypesImplementingInterface(assemblies)
                    .Where(x => !x.FullName!.Contains(nameof(MassTransit)));
                var messageTypeConsumerType = messageTypeConsumerConcretedTypes.SingleOrDefault();

                if (messageTypeConsumerType is null)
                {
                    continue;
                }

                consumerType = messageTypeConsumerType;
            }

            ConfigureMessageReceiveEndpoint(rabbitMqBusFactoryConfigurator, context, messageType, consumerType);
        }
    }

    private static void ConfigureMessageReceiveEndpoint(
        IRabbitMqBusFactoryConfigurator rabbitMqBusFactoryConfigurator,
        IBusRegistrationContext context,
        Type messageType,
        Type consumerType
    )
    {
        // https://github.com/MassTransit/MassTransit/blob/eb3c9ee1007cea313deb39dc7c4eb796b7e61579/src/Transports/MassTransit.RabbitMqTransport/RabbitMqTransport/Configuration/RabbitMqReceiveEndpointBuilder.cs#L70
        // https://spring.io/blog/2011/04/01/routing-topologies-for-performance-and-scalability-with-rabbitmq

        // https://masstransit.io/documentation/transports/rabbitmq
        // This `queueName` creates an `intermediary exchange` (default is Fanout, but we can change it with re.ExchangeType) with the same queue named which bound to this exchange
        rabbitMqBusFactoryConfigurator.ReceiveEndpoint(
            queueName: messageType.Name.Underscore(),
            re =>
            {
                re.Durable = true;

                // set intermediate exchange type
                // intermediate exchange name will be the same as queue name
                re.ExchangeType = ExchangeType.Fanout;

                // a replicated queue to provide high availability and data safety. available in RMQ 3.8+
                re.SetQuorumQueue();

                // with setting `ConfigureConsumeTopology` to `false`, we should create `primary exchange` and its bounded exchange manually with using `re.Bind` otherwise with `ConfigureConsumeTopology=true` it get publish topology for message type `T` with `_publishTopology.GetMessageTopology<T>()` and use its ExchangeType and ExchangeName based ofo default EntityFormatter
                re.ConfigureConsumeTopology = true;

                // // https://spring.io/blog/2011/04/01/routing-topologies-for-performance-and-scalability-with-rabbitmq
                // // masstransit uses `wire-tapping` pattern for defining exchanges. Primary exchange will send the message to intermediary fanout exchange
                // // setup primary exchange and its type
                // re.Bind(
                //     $"{type.Name.Underscore()}{MessagingConstants.PrimaryExchangePostfix}",
                //     e =>
                //     {
                //         e.RoutingKey = type.Name.Underscore();
                //         e.ExchangeType = ExchangeType.Direct;
                //     }
                // );

                // https://github.com/MassTransit/MassTransit/discussions/3117
                // https://masstransit-project.com/usage/configuration.html#receive-endpoints
                re.ConfigureConsumer(context, consumerType);

                re.RethrowFaultedMessages();
            }
        );
    }

    private static void ApplyMessagesPublishTopology(
        IRabbitMqPublishTopologyConfigurator publishTopology,
        Assembly[] assemblies
    )
    {
        // Get all types that implement the IMessage interface
        var messageTypes = ReflectionUtilities
            .GetAllTypesImplementingInterface<IIntegrationEvent>(assemblies)
            .Where(x => !x.IsGenericType);

        foreach (var messageType in messageTypes)
        {
            var eventEnvelopeInterfaceMessageType = typeof(IEventEnvelope<>).MakeGenericType(messageType);
            var eventEnvelopeInterfaceConfigurator = publishTopology.GetMessageTopology(
                eventEnvelopeInterfaceMessageType
            );

            // setup primary exchange
            eventEnvelopeInterfaceConfigurator.Durable = true;
            eventEnvelopeInterfaceConfigurator.ExchangeType = ExchangeType.Direct;

            var eventEnvelopeMessageType = typeof(EventEnvelope<>).MakeGenericType(messageType);
            var eventEnvelopeMessageTypeConfigurator = publishTopology.GetMessageTopology(eventEnvelopeMessageType);
            eventEnvelopeMessageTypeConfigurator.Durable = true;
            eventEnvelopeMessageTypeConfigurator.ExchangeType = ExchangeType.Direct;

            // none event-envelope message types
            var messageConfigurator = publishTopology.GetMessageTopology(messageType);
            messageConfigurator.Durable = true;
            messageConfigurator.ExchangeType = ExchangeType.Direct;
        }
    }

    private static IRetryConfigurator AddRetryConfiguration(IRetryConfigurator retryConfigurator)
    {
        retryConfigurator
            .Exponential(3, TimeSpan.FromMilliseconds(200), TimeSpan.FromMinutes(120), TimeSpan.FromMilliseconds(200))
            .Ignore<ValidationException>(); // don't retry if we have invalid data and message goes to _error queue masstransit

        return retryConfigurator;
    }
}
