using BuildingBlocks.Abstractions.Queries;

namespace BuildingBlocks.Core.Queries;

internal static class DependencyInjectionExtensions
{
    internal static IServiceCollection AddQueryBus(this IServiceCollection services)
    {
        services.AddTransient<IQueryBus, QueryBus>();

        return services;
    }
}
