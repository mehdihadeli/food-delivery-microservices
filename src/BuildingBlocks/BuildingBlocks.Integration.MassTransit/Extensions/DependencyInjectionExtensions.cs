using System.Reflection;
using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Exception;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.HostApplicationBuilderExtensions;
using BuildingBlocks.Core.Extensions.ServiceCollectionExtensions;
using BuildingBlocks.Core.Messages;
using BuildingBlocks.Core.Reflection;
using BuildingBlocks.Core.Web.Extensions;
using Humanizer;
using MassTransit;
using MassTransit.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using Constants = BuildingBlocks.Core.Constants;

namespace BuildingBlocks.Integration.MassTransit.Extensions;

public static class DependencyInjectionExtensions
{
    public static IHostApplicationBuilder AddMasstransitEventBus(
        this IHostApplicationBuilder builder,
        Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator>? configureMessagesTopologies = null,
        Action<IBusRegistrationConfigurator>? configureBusRegistration = null,
        Action<MasstransitOptions>? configureMasstransitOptions = null,
        Assembly[]? assemblies = null
    )
    {
        assemblies ??= [Assembly.GetCallingAssembly()];

        // Add option to the dependency injection
        builder.Services.AddValidationOptions(configurator: configureMasstransitOptions);
        var masstransitOptions = builder.Configuration.BindOptions<MasstransitOptions>();

        // - EnvironmentVariablesConfigurationProvider is injected by aspire and use to read configuration values from environment variables with `ConnectionStrings:redis` key on configuration.
        // The configuration provider handles these conversions automatically, and `__ (double underscore)` becomes `:` for nested sections, so environment configuration reads its data from the ` ConnectionStrings__redis ` environment. all envs are available in `Environment.GetEnvironmentVariables()`.
        // - For setting none sensitive configuration, we can use Aspire named configuration `Aspire:StackExchange:Redis:DisableHealthChecks` which is of type ConfigurationProvider and should be set in appsetting.json

        // https://learn.microsoft.com/en-us/dotnet/aspire/messaging/rabbitmq-integration?tabs=dotnet-cli#use-a-connection-strin
        // first read from aspire injected ConnectionString then read from config
        var connectionString =
            builder.Configuration.GetConnectionString(Constants.AspireResources.Rabbitmq)
            ?? masstransitOptions.RabbitMQConnectionString
            ?? throw new InvalidOperationException(
                $"Connection string '{Constants.AspireResources.Rabbitmq}' or `MasstransitOptions.RabbitMQConnectionString` not found."
            );

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
        builder.Services.Replace(ServiceDescriptor.Transient<IExternalEventBus, MassTransitBus>());
        builder.Services.Replace(ServiceDescriptor.Transient<IBusDirectPublisher, MasstransitDirectPublisher>());

        ConfigureInstrumentation(builder, masstransitOptions, connectionString);

        if (!builder.Environment.IsTest())
        {
            builder.Services.AddMassTransit(cfg =>
                ConfiguratorAction(
                    cfg,
                    connectionString,
                    masstransitOptions,
                    configureMessagesTopologies,
                    configureBusRegistration,
                    assemblies
                )
            );
        }
        else
        {
            builder.Services.AddMassTransitTestHarness(cfg =>
                ConfiguratorAction(
                    cfg,
                    connectionString,
                    masstransitOptions,
                    configureMessagesTopologies,
                    configureBusRegistration,
                    assemblies
                )
            );
        }

        return builder;
    }

    private static void ConfiguratorAction(
        IBusRegistrationConfigurator busRegistrationConfigurator,
        string connectionString,
        MasstransitOptions masstransitOptions,
        Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator>? configureMessagesTopologies = null,
        Action<IBusRegistrationConfigurator>? configureBusRegistration = null,
        Assembly[]? assemblies = null
    )
    {
        // https://masstransit.io/documentation/configuration#health-check-options
        busRegistrationConfigurator.ConfigureHealthCheckOptions(options =>
        {
            options.Name = "masstransit";
            options.MinimalFailureStatus = HealthStatus.Unhealthy;
            options.Tags.Add("live");
        });

        configureBusRegistration?.Invoke(busRegistrationConfigurator);

        // https://masstransit-project.com/usage/configuration.html#receive-endpoints
        busRegistrationConfigurator.AddConsumers(assemblies);

        busRegistrationConfigurator.AddSagas(assemblies);

        busRegistrationConfigurator.AddActivities(assemblies);

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
                cfg.Host(new Uri(connectionString), hostConfigurator => hostConfigurator.PublisherConfirmation = true);

                // retry failure in consumers 3 times by using an inbox-pattern for Transient failures(e.g. database timeout, temporary lock), and using dead-letter-queue for Permanent failures(e.g. invalid message, logical bug) and prevent redelivering forever.
                cfg.UseMessageRetry(r =>
                    r.Exponential(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(5))
                );

                // https: // github.com/MassTransit/MassTransit/issues/2018
                // https://github.com/MassTransit/MassTransit/issues/4831
                cfg.Publish<IIntegrationEvent>(p => p.Exclude = true);
                cfg.Publish<IntegrationEvent>(p => p.Exclude = true);
                cfg.Publish<IMessage>(p => p.Exclude = true);
                cfg.Publish<Message>(p => p.Exclude = true);
                cfg.Publish<ITxRequest>(p => p.Exclude = true);
                cfg.Publish<IMessageEnvelopeBase>(p => p.Exclude = true);

                cfg.UseConsumeFilter(typeof(HeadersPropagationFilter<>), context);
                cfg.UseConsumeFilter(typeof(MessageHandlerConsumerFilter<>), context);

                if (masstransitOptions.AutoConfigEndpoints)
                {
                    // https://masstransit.io/documentation/configuration#consumer-registration
                    // https://masstransit.io/documentation/configuration#configure-endpoints
                    // MassTransit is able to configure receive endpoints for all registered consumer types. Receive endpoint names are generated using an endpoint name formatter
                    cfg.ConfigureEndpoints(context);
                }

                // for setting exchange name for a message type as default. masstransit by default uses fully message type name for primary exchange name.
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

    private static void ConfigureInstrumentation(
        IHostApplicationBuilder builder,
        MasstransitOptions masstransitOptions,
        string connectionString
    )
    {
        if (!masstransitOptions.DisableTracing)
        {
            builder
                .Services.AddOpenTelemetry()
                .WithMetrics(b => b.AddMeter(DiagnosticHeaders.DefaultListenerName))
                .WithTracing(p =>
                {
                    // built-in support for tracing in RabbitMQ.Client v7
                    p.AddSource("RabbitMQ.Client.*");
                    p.AddSource(DiagnosticHeaders.DefaultListenerName);
                });
        }

        if (
            masstransitOptions is
            { MasstransitMessagingProvider: MasstransitMessagingProvider.RabbitMQ, DisableHealthChecks: false }
        )
        {
            builder.TryAddHealthCheck(
                "RabbitMQ.Client",
                healthCheckBuilder =>
                {
                    healthCheckBuilder.AddRabbitMQ(
                        async sp =>
                        {
                            var s = sp.GetRequiredService<IConnection>();
                            var factory = new ConnectionFactory { Uri = new Uri(connectionString) };
                            return await factory.CreateConnectionAsync().ConfigureAwait(false);
                        },
                        name: "RabbitMQ-Check",
                        timeout: TimeSpan.FromSeconds(3),
                        tags: ["live"]
                    );
                }
            );
        }
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
                        $"{messageType.Name.Underscore()}{Constants.MessagingConstants.PrimaryExchangePostfix}",
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

                // Set up dead-letter queue by adding arguments
                re.SetQueueArgument("x-dead-letter-exchange", $"{messageType.Name.Underscore()}_dead_letter_exchange");
                re.SetQueueArgument("x-dead-letter-routing-key", messageType.Name.Underscore());
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
            .Ignore<ValidationException>(); // don't retry if we have invalid data and a message goes to _error queue masstransit

        return retryConfigurator;
    }
}
