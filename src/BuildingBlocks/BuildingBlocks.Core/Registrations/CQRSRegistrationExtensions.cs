using System.Reflection;
using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Abstractions.CQRS.Event;
using BuildingBlocks.Abstractions.CQRS.Event.Internal;
using BuildingBlocks.Abstractions.CQRS.Query;
using BuildingBlocks.Abstractions.Scheduler;
using BuildingBlocks.Core.CQRS.Command;
using BuildingBlocks.Core.CQRS.Event;
using BuildingBlocks.Core.CQRS.Query;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Core.Scheduler;
using MediatR;

namespace BuildingBlocks.Core.Registrations;

public static class CQRSRegistrationExtensions
{
    public static IServiceCollection AddCqrs(
        this IServiceCollection services,
        Assembly[]? assemblies = null,
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient,
        Action<IServiceCollection>? doMoreActions = null)
    {
        services.AddMediatR(
            assemblies ?? new[] { Assembly.GetCallingAssembly() },
            x =>
            {
                switch (serviceLifetime)
                {
                    case ServiceLifetime.Transient:
                        x.AsTransient();
                        break;
                    case ServiceLifetime.Scoped:
                        x.AsScoped();
                        break;
                    case ServiceLifetime.Singleton:
                        x.AsSingleton();
                        break;
                }
            });

        // services.Decorate(typeof(IEventHandler<>), typeof(EventHandlerDecorator<>));

        services.Add<ICommandProcessor, CommandProcessor>(serviceLifetime)
            .Add<IQueryProcessor, QueryProcessor>(serviceLifetime)
            .Add<IEventProcessor, EventProcessor>(serviceLifetime)
            .Add<ICommandScheduler, NullCommandScheduler>(serviceLifetime)
            .Add<IDomainEventPublisher, DomainEventPublisher>(serviceLifetime)
            .Add<IDomainNotificationEventPublisher, DomainNotificationEventPublisher>(serviceLifetime);

        services.AddScoped<IDomainEventsAccessor, NullDomainEventsAccessor>();

        doMoreActions?.Invoke(services);

        return services;
    }
}
