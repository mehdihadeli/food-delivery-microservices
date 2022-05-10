using Humanizer;
using MassTransit;
using Store.Services.Customers.Identity.Features.RegisteringUser.Events.External;
using Store.Services.Shared.Identity.Users.Events.Integration;

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
        cfg.ReceiveEndpoint(nameof(UserRegistered).Underscore() + ".customers", e =>
        {
            e.RethrowFaultedMessages();
            e.ConfigureConsumer<UserRegisteredConsumer>(context);
        });
    }
}
