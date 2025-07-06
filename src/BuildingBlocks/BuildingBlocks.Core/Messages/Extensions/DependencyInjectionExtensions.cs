using System.Reflection;
using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Abstractions.Messages.MessagePersistence;
using BuildingBlocks.Abstractions.Persistence.EventStore;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollectionExtensions;
using BuildingBlocks.Core.Messages.MessagePersistence;
using BuildingBlocks.Core.Messages.MessagePersistence.BackgroundServices;
using BuildingBlocks.Core.Messages.MessagePersistence.InMemory;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Scrutor;

namespace BuildingBlocks.Core.Messages.Extensions;

public static class DependencyInjectionExtensions
{
    internal static void AddMessages(this IServiceCollection services, Assembly[] assemblies)
    {
        services.AddValidationOptions<MessagingOptions>();

        services.AddScoped<IMessageMetadataAccessor, MessageMetadataAccessor>();

        services.AddSingleton<IMessagePublisher, MessagePublisher>();

        // will override by messaging systems like rabbitmq - we should not use `TryAddTransient` for replacing in rabbitmq
        services.AddTransient<IExternalEventBus, NullExternalEventBus>();
        services.AddTransient<IBusDirectPublisher, NullIBusDirectPublisher>();

        AddMessageHandler(services, assemblies);

        AddPersistenceMessage(services);
    }

    private static void AddPersistenceMessage(IServiceCollection services)
    {
        services.TryAddScoped<IMessagePersistenceService, MessagePersistenceService>();
        services.AddHostedService<MessagePersistenceBackgroundService>();
        services.AddValidationOptions<MessagePersistenceOptions>();
        services.AddInMemoryMessagePersistence();
    }

    private static void AddInMemoryMessagePersistence(this IServiceCollection services)
    {
        services.AddScoped<IMessagePersistenceRepository, InMemoryMessagePersistenceRepository>();
    }

    private static void AddMessageHandler(IServiceCollection services, Assembly[] assemblies)
    {
        services.Scan(scan =>
            scan.FromAssemblies(assemblies)
                .AddClasses(classes => classes.AssignableTo(typeof(IMessageHandler<>)))
                .UsingRegistrationStrategy(RegistrationStrategy.Append)
                .AsClosedTypeOf(typeof(IMessageHandler<>))
                .AsSelf()
                .WithLifetime(ServiceLifetime.Transient)
        );

        services.Scan(scan =>
            scan.FromAssemblies(assemblies)
                .AddClasses(classes => classes.AssignableTo(typeof(IMessageEnvelopeHandler<>)))
                .UsingRegistrationStrategy(RegistrationStrategy.Append)
                .AsClosedTypeOf(typeof(IMessageEnvelopeHandler<>))
                .AsSelf()
                .WithLifetime(ServiceLifetime.Transient)
        );

        services.Scan(scan =>
            scan.FromAssemblies(assemblies)
                .AddClasses(classes => classes.AssignableTo(typeof(IStreamEventEnvelope<>)))
                .UsingRegistrationStrategy(RegistrationStrategy.Append)
                .AsClosedTypeOf(typeof(IStreamEventEnvelope<>))
                .AsSelf()
                .WithLifetime(ServiceLifetime.Transient)
        );
    }
}
