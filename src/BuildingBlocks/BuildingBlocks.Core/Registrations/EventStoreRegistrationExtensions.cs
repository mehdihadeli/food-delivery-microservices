using System.Reflection;
using BuildingBlocks.Abstractions.Domain.Events;
using BuildingBlocks.Abstractions.Persistence.EventStore;
using BuildingBlocks.Abstractions.Persistence.EventStore.Projections;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Core.Persistence.EventStore;
using BuildingBlocks.Core.Persistence.EventStore.InMemory;
using BuildingBlocks.Core.Reflection;
using BuildingBlocks.Persistence.EventStoreDB;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlocks.Core.Registrations;

public static class EventStoreRegistrationExtensions
{
    public static IServiceCollection AddInMemoryEventStore(this IServiceCollection services)
    {
        return AddEventSourcing<InMemoryEventStore>(services, ServiceLifetime.Singleton);
    }

    public static IServiceCollection AddEventSourcing<TEventStore>(
        this IServiceCollection services,
        ServiceLifetime withLifetime = ServiceLifetime.Scoped,
        params Assembly[] scanAssemblies
    )
        where TEventStore : class, IEventStore
    {
        var assembliesToScan = scanAssemblies.Any() ? scanAssemblies : new[] { Assembly.GetCallingAssembly(), };

        services.Add<IAggregateStore, AggregateStore>(withLifetime);
        services.Add<IDomainEventsAccessor, EventStoreDomainEventAccessor>(withLifetime);

        services
            .Add<TEventStore, TEventStore>(withLifetime)
            .Add<IEventStore>(sp => sp.GetRequiredService<TEventStore>(), withLifetime);

        services.AddReadProjections(assembliesToScan);

        return services;
    }

    private static IServiceCollection AddReadProjections(
        this IServiceCollection services,
        params Assembly[] scanAssemblies
    )
    {
        services.TryAddSingleton<IReadProjectionPublisher, ReadProjectionPublisher>();

        // Assemblies are lazy loaded so using AppDomain.GetAssemblies is not reliable.
        var assemblies = scanAssemblies.Any()
            ? scanAssemblies
            : ReflectionUtilities.GetReferencedAssemblies(Assembly.GetCallingAssembly()).ToArray();

        RegisterProjections(services, assemblies!);

        return services;
    }

    private static void RegisterProjections(IServiceCollection services, Assembly[] assembliesToScan)
    {
        services.Scan(
            scan =>
                scan.FromAssemblies(assembliesToScan)
                    .AddClasses(classes => classes.AssignableTo<IHaveReadProjection>()) // Filter classes
                    .AsImplementedInterfaces()
                    .WithTransientLifetime()
        );
    }
}
