using System.Reflection;
using BuildingBlocks.Abstractions.Persistence.EventStore;
using BuildingBlocks.Abstractions.Persistence.EventStore.Projections;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Persistence.EventStore.InMemory;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlocks.Core.Persistence.EventStore.Extensions;

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
        // Assemblies are lazy loaded so using AppDomain.GetAssemblies is not reliable (it is possible to get ReflectionTypeLoadException, because some dependent type assembly are lazy and not loaded yet), so we use `GetAllReferencedAssemblies` and it
        // loads all referenced assemblies explicitly.
        if (scanAssemblies.Length == 0)
        {
            // Find assemblies that reference the current assembly
            var referencingAssemblies = Assembly.GetExecutingAssembly().GetReferencingAssemblies();
            scanAssemblies = referencingAssemblies.ToArray();
        }

        services
            .AddSingleton<IAggregateStore, AggregateStore>()
            .AddSingleton<TEventStore, TEventStore>()
            .AddSingleton<IEventStore>(sp => sp.GetRequiredService<TEventStore>());

        services.AddReadProjections(scanAssemblies);

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
