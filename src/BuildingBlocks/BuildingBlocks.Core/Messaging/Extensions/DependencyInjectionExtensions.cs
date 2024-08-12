using System.Reflection;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Core.Messaging.BackgroundServices;
using BuildingBlocks.Core.Messaging.MessagePersistence;
using BuildingBlocks.Core.Messaging.MessagePersistence.InMemory;
using BuildingBlocks.Core.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Scrutor;

namespace BuildingBlocks.Core.Messaging.Extensions;

public static class DependencyInjectionExtensions
{
    internal static void AddMessagingCore(this IServiceCollection services, params Assembly[] scanAssemblies)
    {
        var assemblies =
            scanAssemblies.Length != 0
                ? scanAssemblies
                : ReflectionUtilities.GetReferencedAssemblies(Assembly.GetCallingAssembly()).Distinct().ToArray();

        services.AddScoped<IMessageMetadataAccessor, MessageMetadataAccessor>();
        AddMessagingMediator(services, assemblies);

        AddPersistenceMessage(services);
    }

    private static void AddPersistenceMessage(IServiceCollection services)
    {
        services.TryAddScoped<IMessagePersistenceService, MessagePersistenceService>();
        services.AddHostedService<MessagePersistenceBackgroundService>();
        services.AddValidatedOptions<MessagePersistenceOptions>();
        services.AddInMemoryMessagePersistence();
    }

    private static IServiceCollection AddInMemoryMessagePersistence(this IServiceCollection services)
    {
        services.Replace(
            ServiceDescriptor.Scoped<IMessagePersistenceRepository, InMemoryMessagePersistenceRepository>()
        );

        return services;
    }

    private static void AddMessagingMediator(IServiceCollection services, Assembly[] scanAssemblies)
    {
        services.Scan(scan =>
            scan.FromAssemblies(scanAssemblies)
                .AddClasses(classes => classes.AssignableTo(typeof(IMessageHandler<>)))
                .UsingRegistrationStrategy(RegistrationStrategy.Append)
                .AsClosedTypeOf(typeof(IMessageHandler<>))
                .AsSelf()
                .WithLifetime(ServiceLifetime.Transient)
        );
    }
}
