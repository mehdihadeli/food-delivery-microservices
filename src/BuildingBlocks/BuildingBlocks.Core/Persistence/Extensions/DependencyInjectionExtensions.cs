using System.Reflection;
using BuildingBlocks.Abstractions.Persistence;

namespace BuildingBlocks.Core.Persistence.Extensions;

internal static class DependencyInjectionExtensions
{
    internal static IServiceCollection AddPersistenceCore(
        this IServiceCollection services,
        params Assembly[] assembliesToScan
    )
    {
        services.ScanAndRegisterDbExecutors(assembliesToScan);

        services.AddHostedService<SeedWorker>();
        services.AddScoped<IMigrationManager, MigrationManager>();

        return services;
    }
}
