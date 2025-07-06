using BuildingBlocks.Abstractions.Queries;
using BuildingBlocks.Core.Queries.Diagnostics;

namespace BuildingBlocks.Core.Queries;

internal static class DependencyInjectionExtensions
{
    internal static IServiceCollection AddQueryBus(this IServiceCollection services)
    {
        services.AddTransient<IQueryBus, QueryBus>();

        services.AddTransient<QueryHandlerActivity>();
        services.AddTransient<QueryHandlerMetrics>();

        return services;
    }
}
