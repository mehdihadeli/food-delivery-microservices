using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Scheduler;
using BuildingBlocks.Core.Commands.Diagnostics;
using BuildingBlocks.Core.Scheduler;

namespace BuildingBlocks.Core.Commands;

internal static class DependencyInjectionExtensions
{
    public static IServiceCollection AddCommandBus(this IServiceCollection services)
    {
        services.AddTransient<ICommandBus, CommandBus>();
        services.AddTransient<IAsyncCommandBus, AsyncCommandBus>();
        services.AddTransient<ICommandScheduler, NullCommandScheduler>();

        services.AddTransient<CommandHandlerActivity>();
        services.AddTransient<CommandHandlerMetrics>();

        return services;
    }
}
