using Ardalis.GuardClauses;
using BuildingBlocks.Core.Extensions;
using MassTransit;
using Microsoft.Extensions.Configuration;
using IBus = BuildingBlocks.Abstractions.Messaging.IBus;

namespace BuildingBlocks.Integration.MassTransit;

public static class Extensions
{
    public static IServiceCollection AddCustomMassTransit(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? customBusRegistrationConfigurator = null,
        Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator>? configureReceiveEndpoints = null)
    {
        var rabbitMqOptions = configuration.GetOptions<RabbitMqOptions>(nameof(RabbitMqOptions));

        Guard.Against.Null(rabbitMqOptions, nameof(rabbitMqOptions));

        services.AddMassTransit(busRegistrationConfigurator =>
        {
            // busRegistrationConfigurator.SetKebabCaseEndpointNameFormatter();

            // exclude namespace for the messages
            busRegistrationConfigurator.SetEndpointNameFormatter(new SnakeCaseEndpointNameFormatter(false));

            customBusRegistrationConfigurator?.Invoke(busRegistrationConfigurator);

            busRegistrationConfigurator.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqOptions.Host, hostConfigurator =>
                {
                    hostConfigurator.Username(rabbitMqOptions.UserName);
                    hostConfigurator.Password(rabbitMqOptions.Password);
                });

                cfg.MessageTopology.SetEntityNameFormatter(new CustomEntityNameFormatter());
                configureReceiveEndpoints?.Invoke(context, cfg);
            });
        });

        services.AddTransient<IBus, MassTransitBus>();

        return services;
    }
}
