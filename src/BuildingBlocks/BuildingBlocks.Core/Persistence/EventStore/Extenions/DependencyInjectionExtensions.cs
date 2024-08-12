using System.Reflection;
using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Persistence.EventStore;
using BuildingBlocks.Abstractions.Persistence.EventStore.Projections;
using BuildingBlocks.Core.Persistence.EventStore.InMemory;
using BuildingBlocks.Core.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlocks.Core.Persistence.EventStore.Extenions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInMemoryEventStore(this IServiceCollection services)
    {
        return AddEventSourcing<InMemoryEventStore>(services);
    }

    public static IServiceCollection AddEventSourcing<TEventStore>(
        this IServiceCollection services,
        params Assembly[] scanAssemblies
    )
        where TEventStore : class, IEventStore
    {
        // Assemblies are lazy loaded so using AppDomain.GetAssemblies is not reliable.
        var assemblies =
            scanAssemblies.Length != 0
                ? scanAssemblies
                : ReflectionUtilities.GetReferencedAssemblies(Assembly.GetCallingAssembly()).ToArray();

        services.Replace(ServiceDescriptor.Scoped<IDomainEventsAccessor, EventStoreDomainEventAccessor>());

        services
            .AddSingleton<IAggregateStore, AggregateStore>()
            .AddSingleton<TEventStore, TEventStore>()
            .AddSingleton<IEventStore>(sp => sp.GetRequiredService<TEventStore>());

        services.AddReadProjections(assemblies);

        return services;
    }

    private static IServiceCollection AddReadProjections(
        this IServiceCollection services,
        params Assembly[] scanAssemblies
    )
    {
        services.TryAddSingleton<IReadProjectionPublisher, ReadProjectionPublisher>();

        RegisterProjections(services, scanAssemblies!);

        return services;
    }

    private static void RegisterProjections(IServiceCollection services, Assembly[] assembliesToScan)
    {
        services.Scan(scan =>
            scan.FromAssemblies(assembliesToScan)
                .AddClasses(classes => classes.AssignableTo<IHaveReadProjection>()) // Filter classes
                .AsImplementedInterfaces()
                .WithTransientLifetime()
        );
    }
}
