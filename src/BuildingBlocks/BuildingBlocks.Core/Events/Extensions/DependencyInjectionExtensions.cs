namespace BuildingBlocks.Core.Events.Extensions;

using System.Reflection;
using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Core.Extensions;

internal static class DependencyInjectionExtensions
{
    internal static IServiceCollection AddEvents(this IServiceCollection services, Assembly[] assemblies)
    {
        services
            .AddTransient<IDomainEventPublisher, DomainEventPublisher>()
            .AddTransient<IDomainNotificationEventPublisher, DomainNotificationEventPublisher>()
            .AddTransient<IInternalEventBus, InternalEventBus>();

        services.AddScoped<IAggregatesDomainEventsRequestStorage, AggregatesDomainEventsStorage>();
        services.AddScoped<IDomainEventsAccessor, DomainEventAccessor>();

        // will override by services using their dbcontext
        services.AddScoped<IDomainEventContext, NullIDomainEventContext>();

        RegisterEventMappers(services, assemblies);

        return services;
    }

    private static void RegisterEventMappers(IServiceCollection services, Assembly[] scanAssemblies)
    {
        services.Scan(scan =>
            scan.FromAssemblies(scanAssemblies)
                .AddClasses(classes => classes.AssignableTo<IEventMapper>(), false)
                .AsImplementedInterfaces()
                .WithSingletonLifetime()
                .AddClasses(classes => classes.AssignableTo<IIntegrationEventMapper>(), false)
                .AsImplementedInterfaces()
                .WithSingletonLifetime()
                .AddClasses(classes => classes.AssignableTo<IDomainNotificationEventMapper>(), false)
                .AsImplementedInterfaces()
                .WithSingletonLifetime()
        );
    }
}
