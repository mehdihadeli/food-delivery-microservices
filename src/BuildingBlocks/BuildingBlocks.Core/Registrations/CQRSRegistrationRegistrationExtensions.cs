using System.Reflection;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Abstractions.Domain.Events;
using BuildingBlocks.Abstractions.Domain.Events.Internal;
using BuildingBlocks.Abstractions.Scheduler;
using BuildingBlocks.Core.CQRS.Commands;
using BuildingBlocks.Core.CQRS.Queries;
using BuildingBlocks.Core.Domain.Events;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Core.Scheduler;
using MediatR;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlocks.Core.Registrations;

public static class CQRSRegistrationRegistrationExtensions
{
    public static IServiceCollection AddCqrs(
        this IServiceCollection services,
        Assembly[]? assemblies = null,
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient,
        params Type[] pipelines
    )
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
            }
        );

        foreach (var pipeline in pipelines)
        {
            services.TryAddScoped(typeof(IPipelineBehavior<,>), pipeline);
        }

        services
            .Add<ICommandProcessor, CommandProcessor>(serviceLifetime)
            .Add<IQueryProcessor, QueryProcessor>(serviceLifetime)
            .Add<ICommandScheduler, NullCommandScheduler>(serviceLifetime)
            .Add<IInternalEventBus, InternalEventBus>();

        return services;
    }
}
