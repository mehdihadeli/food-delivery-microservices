using System.Reflection;
using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Events.Internal;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Persistence.EventStore;
using BuildingBlocks.Core.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlocks.Core.Events.Extensions;

internal static class DependencyInjectionExtensions
{
    internal static IServiceCollection AddEventBus(this IServiceCollection services, params Assembly[] scanAssemblies)
    {
        var assemblies =
            scanAssemblies.Length != 0
                ? scanAssemblies
                : ReflectionUtilities.GetReferencedAssemblies(Assembly.GetCallingAssembly()).Distinct().ToArray();

        services
            .AddTransient<IDomainEventPublisher, DomainEventPublisher>()
            .AddTransient<IDomainNotificationEventPublisher, DomainNotificationEventPublisher>()
            .AddTransient<IInternalEventBus, InternalEventBus>();

        services.AddTransient<IAggregatesDomainEventsRequestStorage, AggregatesDomainEventsStorage>();
        services.AddScoped<IDomainEventsAccessor, DomainEventAccessor>();

        RegisterEventMappers(services, assemblies);

        return services;
    }

    private static void RegisterEventMappers(IServiceCollection services, Assembly[] scanAssemblies)
    {
        services.Scan(scan =>
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
}
