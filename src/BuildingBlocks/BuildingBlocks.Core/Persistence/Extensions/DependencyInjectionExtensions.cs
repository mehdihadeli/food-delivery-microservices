using System.Reflection;
using BuildingBlocks.Abstractions.Persistence;

namespace BuildingBlocks.Core.Persistence.Extensions;

public static class DependencyInjectionExtensions
{
    internal static IServiceCollection AddPersistenceCore(
        this IServiceCollection services,
        params Assembly[] assembliesToScan
    )
    {
        services.ScanAndRegisterDbExecutors(assembliesToScan);

        services.RegisterDataSeeders(assembliesToScan);

        services.AddHostedService<SeedWorker>();
        services.AddScoped<IMigrationManager, MigrationManager>();

        return services;
    }

    public static void RegisterDataSeeders(this IServiceCollection services, Assembly[] assembliesToScan)
    {
        services.Scan(scan =>
            scan.FromAssemblies(assembliesToScan)
                .AddClasses(classes => classes.AssignableTo<IDataSeeder>())
                .AsImplementedInterfaces()
                .WithScopedLifetime()
                .AddClasses(classes => classes.AssignableTo<ITestDataSeeder>())
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );
    }
}
