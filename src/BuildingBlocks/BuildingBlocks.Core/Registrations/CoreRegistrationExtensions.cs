using System.Reflection;
using BuildingBlocks.Abstractions.Core;
using BuildingBlocks.Abstractions.Domain.Events;
using BuildingBlocks.Abstractions.Domain.Events.Internal;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Abstractions.Serialization;
using BuildingBlocks.Abstractions.Types;
using BuildingBlocks.Core.Domain;
using BuildingBlocks.Core.Domain.Events;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Core.IdsGenerator;
using BuildingBlocks.Core.Messaging.BackgroundServices;
using BuildingBlocks.Core.Messaging.MessagePersistence;
using BuildingBlocks.Core.Messaging.MessagePersistence.InMemory;
using BuildingBlocks.Core.Paging;
using BuildingBlocks.Core.Persistence;
using BuildingBlocks.Core.Reflection;
using BuildingBlocks.Core.Serialization;
using BuildingBlocks.Core.Types;
using BuildingBlocks.Core.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;
using Scrutor;
using Sieve.Services;

namespace BuildingBlocks.Core.Registrations;

public static class CoreRegistrationExtensions
{
    public static IServiceCollection AddCore(this IServiceCollection services, params Assembly[] scanAssemblies)
    {
        var systemInfo = MachineInstanceInfo.New();

        // Assemblies are lazy loaded so using AppDomain.GetAssemblies is not reliable (it is possible to get ReflectionTypeLoadException, because some dependent type assembly are lazy and not loaded yet), so we use `GetAllReferencedAssemblies` and it
        // load all referenced assemblies explicitly.
        var assemblies = scanAssemblies.Any()
            ? scanAssemblies
            : ReflectionUtilities.GetReferencedAssemblies(Assembly.GetCallingAssembly()).Distinct().ToArray();

        services.TryAddSingleton<IMachineInstanceInfo>(systemInfo);
        services.TryAddSingleton(systemInfo);
        services.TryAddSingleton<IExclusiveLock, ExclusiveLock>();

        services.TryAddTransient<IAggregatesDomainEventsRequestStore, AggregatesDomainEventsStore>();
        services.TryAddTransient<IDomainEventsAccessor, DomainEventAccessor>();
        services.TryAddTransient<IDomainEventPublisher, DomainEventPublisher>();
        services.TryAddTransient<IDomainNotificationEventPublisher, DomainNotificationEventPublisher>();

        services.TryAddScoped<ISieveProcessor, ApplicationSieveProcessor>();
        services.ScanAndRegisterDbExecutors(assemblies);

        var policy = Policy.Handle<System.Exception>().RetryAsync(2);
        services.TryAddSingleton<AsyncPolicy>(policy);

        services.AddHttpContextAccessor();

        AddDefaultSerializer(services);

        AddMessagingCore(services, assemblies);

        RegisterEventMappers(services, assemblies);

        RegisterMigrationAndSeedManager(services);

        return services;
    }

    private static void RegisterMigrationAndSeedManager(IServiceCollection services)
    {
        services.AddHostedService<SeedWorker>();
        services.TryAddScoped<IMigrationManager, MigrationManager>();
    }

    private static void RegisterEventMappers(IServiceCollection services, Assembly[] scanAssemblies)
    {
        services.Scan(
            scan =>
                scan.FromAssemblies(scanAssemblies)
                    .AddClasses(classes => classes.AssignableTo(typeof(IEventMapper)), false)
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime()
                    .AddClasses(classes => classes.AssignableTo(typeof(IIntegrationEventMapper)), false)
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime()
                    .AddClasses(classes => classes.AssignableTo(typeof(IIDomainNotificationEventMapper)), false)
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime()
        );
    }

    private static void AddMessagingCore(
        this IServiceCollection services,
        Assembly[] scanAssemblies,
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient
    )
    {
        AddMessagingMediator(services, serviceLifetime, scanAssemblies);

        AddPersistenceMessage(services);
    }

    private static void AddPersistenceMessage(IServiceCollection services)
    {
        services.TryAddScoped<IMessagePersistenceService, MessagePersistenceService>();
        services.TryAddScoped<IMessagePersistenceRepository, NullPersistenceRepository>();
        services.AddHostedService<MessagePersistenceWorker>();
        services.AddValidatedOptions<MessagePersistenceOptions>();
    }

    private static void AddMessagingMediator(
        IServiceCollection services,
        ServiceLifetime serviceLifetime,
        Assembly[] scanAssemblies
    )
    {
        services.Scan(
            scan =>
                scan.FromAssemblies(scanAssemblies)
                    .AddClasses(classes => classes.AssignableTo(typeof(IMessageHandler<>)))
                    .UsingRegistrationStrategy(RegistrationStrategy.Append)
                    .AsClosedTypeOf(typeof(IMessageHandler<>))
                    .AsSelf()
                    .WithLifetime(serviceLifetime)
        );
    }

    private static void AddDefaultSerializer(
        IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Transient
    )
    {
        services.Add<ISerializer, DefaultSerializer>(lifetime);
        services.Add<IMessageSerializer, DefaultMessageSerializer>(lifetime);
    }
}
