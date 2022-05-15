using Humanizer;
using MassTransit;
using RabbitMQ.Client;
using Store.Services.Customers.Identity.Features.RegisteringUser.Events.External;
using Store.Services.Shared.Identity.Users.Events.Integration;

namespace Store.Services.Customers.Identity;

internal static class MassTransitExtensions
{
    internal static void AddIdentityEndpoints(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
    {
        cfg.ReceiveEndpoint(nameof(UserRegistered).Underscore(), re =>
        {
            // turns off default fanout settings
            re.ConfigureConsumeTopology = false;

            // a replicated queue to provide high availability and data safety. available in RMQ 3.8+
            re.SetQuorumQueue();

            re.Bind($"{nameof(UserRegistered).Underscore()}.input_exchange", e =>
            {
                e.RoutingKey = nameof(UserRegistered).Underscore();
                e.ExchangeType = ExchangeType.Direct;
            });

            // https://github.com/MassTransit/MassTransit/discussions/3117
            // https://masstransit-project.com/usage/configuration.html#receive-endpoints
            re.ConfigureConsumer<UserRegisteredConsumer>(context);

            re.RethrowFaultedMessages();
        });
    }
}
