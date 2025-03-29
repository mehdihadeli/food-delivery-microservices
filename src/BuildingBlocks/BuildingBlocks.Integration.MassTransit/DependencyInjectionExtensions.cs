using System.Reflection;
using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Exception.Types;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollectionExtensions;
using BuildingBlocks.Core.Messages;
using BuildingBlocks.Core.Reflection;
using BuildingBlocks.Core.Web.Extensions;
using Humanizer;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace BuildingBlocks.Integration.MassTransit;

public static class DependencyInjectionExtensions
{
    public static WebApplicationBuilder AddMasstransitEventBus(
        this WebApplicationBuilder builder,
        Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator>? configureMessagesTopologies = null,
        Action<IBusRegistrationConfigurator>? configureBusRegistration = null,
        Action<RabbitMqOptions>? configurator = null,
        Action<MasstransitOptions>? configureMasstransitOptions = null
    )
    {
        // Add option to the dependency injection
        builder.Services.AddValidationOptions(configurator: configureMasstransitOptions);
        builder.Services.AddValidationOptions(configurator: configurator);

        var masstransitOptions = builder.Configuration.BindOptions(configureMasstransitOptions);

        // Find all referenced assemblies to current assemblies
        var assemblies = Assembly.GetCallingAssembly().GetReferencingAssemblies();

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
                    // https: // github.com/MassTransit/MassTransit/issues/2018
                    // https://github.com/MassTransit/MassTransit/issues/4831
                    cfg.Publish<IIntegrationEvent>(p => p.Exclude = true);
                    cfg.Publish<IntegrationEvent>(p => p.Exclude = true);
                    cfg.Publish<IMessage>(p => p.Exclude = true);
                    cfg.Publish<Message>(p => p.Exclude = true);
                    cfg.Publish<ITxRequest>(p => p.Exclude = true);
                    cfg.Publish<IMessageEnvelopeBase>(p => p.Exclude = true);

                    cfg.UseConsumeFilter(typeof(HeadersPropagationFilter<>), context);
                    // cfg.UseConsumeFilter(typeof(MessageHandlerConsumerFilter<>), context);

                    if (masstransitOptions.AutoConfigEndpoints)
                    {
                        // https://masstransit.io/documentation/configuration#consumer-registration
                        // https://masstransit.io/documentation/configuration#configure-endpoints
                        // MassTransit is able to configure receive endpoints for all registered consumer types. Receive endpoint names are generated using an endpoint name formatter
                        cfg.ConfigureEndpoints(context);
                    }

                    var rabbitMqOptions = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

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

                    if (masstransitOptions.AutoConfigMessagesTopology)
                    {
                        ApplyMessagesPublishTopology(cfg.PublishTopology, assemblies);
                        ApplyMessagesConsumeTopology(cfg, context, masstransitOptions, assemblies);
                        ApplyMessagesSendTopology(cfg.SendTopology, assemblies);
                    }

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

        // will override default null messaging types - we should not use `TryAddTransient` to replace null types
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
        MasstransitOptions masstransitOptions,
        Assembly[] assemblies
    )
    {
        var consumeTopology = rabbitMqBusFactoryConfigurator.ConsumeTopology;

        var messageTypes = ReflectionUtilities
            .GetAllTypesImplementingInterface<IMessage>(assemblies)
            .Where(x => !x.IsGenericType);

        foreach (var messageType in messageTypes)
        {
            // none event-envelope message types
            var messageConfigurator = consumeTopology.GetMessageTopology(messageType);
            messageConfigurator.ConfigureConsumeTopology = masstransitOptions.ConfigureConsumeTopology;

            var messageConsumerInterface = typeof(IConsumer<>).MakeGenericType(messageType);
            var messageTypeConsumerConcretedTypes = messageConsumerInterface
                .GetAllTypesImplementingInterface(assemblies)
                .Where(x => !x.FullName!.Contains(nameof(MassTransit), StringComparison.InvariantCulture));

            var messageTypeConsumerType = messageTypeConsumerConcretedTypes.SingleOrDefault();
            if (messageTypeConsumerType is null)
            {
                continue;
            }

            ConfigureMessageReceiveEndpoint(
                rabbitMqBusFactoryConfigurator,
                context,
                messageType,
                messageTypeConsumerType,
                masstransitOptions
            );
        }
    }

    private static void ConfigureMessageReceiveEndpoint(
        IRabbitMqBusFactoryConfigurator rabbitMqBusFactoryConfigurator,
        IBusRegistrationContext context,
        Type messageType,
        Type consumerType,
        MasstransitOptions masstransitOptions
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

                // with setting `ConfigureConsumeTopology` to `false`, we should create `primary exchange` and its bounded exchange manually with using `re.Bind` otherwise with `ConfigureConsumeTopology=true` it get publish topology for message type `T` with `_publishTopology.GetMessageTopology<T>()` and use its ExchangeType and ExchangeName based ofo default EntityFormatter but routing-key doesn't set.
                re.ConfigureConsumeTopology = masstransitOptions.ConfigureConsumeTopology;

                if (!masstransitOptions.ConfigureConsumeTopology)
                {
                    // https://spring.io/blog/2011/04/01/routing-topologies-for-performance-and-scalability-with-rabbitmq
                    // masstransit uses `wire-tapping` pattern for defining exchanges. Primary exchange will send the message to intermediary fanout exchange
                    // setup primary exchange and its type
                    re.Bind(
                        $"{messageType.Name.Underscore()}{MessagingConstants.PrimaryExchangePostfix}",
                        e =>
                        {
                            e.RoutingKey = messageType.Name.Underscore();
                            e.ExchangeType = ExchangeType.Direct;
                        }
                    );
                }

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
            .GetAllTypesImplementingInterface<IMessage>(assemblies)
            .Where(x => !x.IsGenericType);

        foreach (var messageType in messageTypes)
        {
            // setup primary exchange
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
