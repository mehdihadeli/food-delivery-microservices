using System.Reflection;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.CQRS.Events;
using BuildingBlocks.Abstractions.CQRS.Events.Internal;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Abstractions.Scheduler;
using BuildingBlocks.Core.CQRS.Commands;
using BuildingBlocks.Core.CQRS.Events;
using BuildingBlocks.Core.CQRS.Queries;
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
        params Type[] pipelines)
    {
        services.AddMediatR(
            assemblies ?? new[] {Assembly.GetCallingAssembly()},
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

        foreach (var pipeline in pipelines)
        {
            services.AddScoped(typeof(IPipelineBehavior<,>), pipeline);
        }

        services
            .Add<ICommandProcessor, CommandProcessor>(serviceLifetime)
            .Add<IQueryProcessor, QueryProcessor>(serviceLifetime)
            .Add<IEventProcessor, EventProcessor>(serviceLifetime)
            .Add<ICommandScheduler, NullCommandScheduler>(serviceLifetime)
            .Add<IDomainEventPublisher, DomainEventPublisher>(serviceLifetime)
            .Add<IDomainNotificationEventPublisher, DomainNotificationEventPublisher>(serviceLifetime);

        services.AddScoped<IDomainEventsAccessor, NullDomainEventsAccessor>();

        return services;
    }
}
