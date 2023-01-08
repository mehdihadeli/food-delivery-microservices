using System.Reflection;
using BuildingBlocks.Abstractions.Core;
using BuildingBlocks.Abstractions.CQRS.Events;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Abstractions.Serialization;
using BuildingBlocks.Abstractions.Types;
using BuildingBlocks.Core.CQRS.Events;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Core.IdsGenerator;
using BuildingBlocks.Core.Messaging.BackgroundServices;
using BuildingBlocks.Core.Messaging.MessagePersistence;
using BuildingBlocks.Core.Messaging.MessagePersistence.InMemory;
using BuildingBlocks.Core.Serialization;
using BuildingBlocks.Core.Types;
using BuildingBlocks.Core.Utils;
using Microsoft.Extensions.Configuration;
using Scrutor;

namespace BuildingBlocks.Core.Registrations;

public static class CoreRegistrationExtensions
{
    public static IServiceCollection AddCore(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] scanAssemblies)
    {
        var systemInfo = MachineInstanceInfo.New();

        // Assemblies are lazy loaded so using AppDomain.GetAssemblies is not reliable (it is possible to get ReflectionTypeLoadException, because some dependent type assembly are lazy and not loaded yet), so we use `GetAllReferencedAssemblies` and it
        // load all referenced assemblies explicitly.
        var assemblies = scanAssemblies.Any()
            ? scanAssemblies
            : ReflectionUtilities.GetReferencedAssemblies(Assembly.GetCallingAssembly())
                .Distinct()
                .ToArray();

        services.AddSingleton<IMachineInstanceInfo>(systemInfo);
        services.AddSingleton(systemInfo);
        services.AddSingleton<IExclusiveLock, ExclusiveLock>();

        services.AddTransient<IAggregatesDomainEventsRequestStore, AggregatesDomainEventsStore>();

        services.AddHttpContextAccessor();

        AddDefaultSerializer(services);

        AddMessagingCore(services, configuration, assemblies);

        RegisterEventMappers(services, assemblies);

        switch (configuration["IdGenerator:Type"])
        {
            case "Guid":
                services.AddSingleton<IIdGenerator<Guid>, GuidIdGenerator>();
                break;
            default:
                services.AddSingleton<IIdGenerator<long>, SnowFlakIdGenerator>();
                break;
        }

        return services;
    }

    private static void RegisterEventMappers(IServiceCollection services, Assembly[] scanAssemblies)
    {
        services.Scan(scan => scan
            .FromAssemblies(scanAssemblies)
            .AddClasses(classes => classes.AssignableTo(typeof(IEventMapper)), false)
            .AsImplementedInterfaces()
            .WithSingletonLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IIntegrationEventMapper)), false)
            .AsImplementedInterfaces()
            .WithSingletonLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IIDomainNotificationEventMapper)), false)
            .AsImplementedInterfaces()
            .WithSingletonLifetime());
    }

    private static void AddMessagingCore(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly[] scanAssemblies,
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        AddMessagingMediator(services, serviceLifetime, scanAssemblies);

        AddPersistenceMessage(services, configuration);
    }

    private static void AddPersistenceMessage(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IMessagePersistenceService, MessagePersistenceService>();
        services.AddScoped<IMessagePersistenceRepository, NullPersistenceRepository>();
        services.AddHostedService<MessagePersistenceBackgroundService>();
        services.AddOptions<MessagePersistenceOptions>()
            .Bind(configuration.GetSection(nameof(MessagePersistenceOptions)))
            .ValidateDataAnnotations();
    }

    private static void AddMessagingMediator(
        IServiceCollection services,
        ServiceLifetime serviceLifetime,
        Assembly[] scanAssemblies)
    {
        services.Scan(scan => scan
            .FromAssemblies(scanAssemblies)
            .AddClasses(classes => classes.AssignableTo(typeof(IMessageHandler<>)))
            .UsingRegistrationStrategy(RegistrationStrategy.Append)
            .AsClosedTypeOf(typeof(IMessageHandler<>))
            .AsSelf()
            .WithLifetime(serviceLifetime));
    }

    private static void AddDefaultSerializer(
        IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        services.Add<ISerializer, DefaultSerializer>(lifetime);
        services.Add<IMessageSerializer, DefaultMessageSerializer>(lifetime);
    }
}
