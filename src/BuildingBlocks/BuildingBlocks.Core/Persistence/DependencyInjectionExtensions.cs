using System.Reflection;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Extensions;

namespace BuildingBlocks.Core.Persistence;

public static class DependencyInjectionExtensions
{
    internal static IServiceCollection AddPersistenceCore(this IServiceCollection services, Assembly[] assemblies)
    {
        services.AddDataSeeders(assemblies);
        services.AddDataMigrationSchemas(assemblies);
        services.ScanAndRegisterDbExecutors(assemblies);

        // registration order is important in the workers and running order is reverse
        services.AddHostedService<MigrationWorker>();
        services.AddHostedService<DataSeedWorker>();

        services.AddSingleton<IMigrationManager, MigrationManager>();
        services.AddSingleton<IDataSeederManager, DataSeederManager>();

        return services;
    }

    private static void ScanAndRegisterDbExecutors(this IServiceCollection services, params Assembly[] assembliesToScan)
    {
        var dbExecutors = assembliesToScan
            .SelectMany(x => x.GetLoadableTypes())
            .Where(t =>
                t!.IsClass
                && !t.IsAbstract
                && !t.IsGenericType
                && !t.IsInterface
                && t.GetConstructor(Type.EmptyTypes) != null
                && typeof(IDbExecutors).IsAssignableFrom(t)
            )
            .ToList();

        foreach (var dbExecutor in dbExecutors)
        {
            var instantiatedType = (IDbExecutors)Activator.CreateInstance(dbExecutor)!;
            instantiatedType.Register(services);
        }
    }

    private static void AddDataSeeders(this IServiceCollection services, IEnumerable<Assembly> assemblies)
    {
        services.Scan(scan =>
            scan.FromAssemblies(assemblies)
                .AddClasses(classes => classes.AssignableTo<IDataSeeder>())
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );
    }

    private static void AddDataMigrationSchemas(this IServiceCollection services, IEnumerable<Assembly> assemblies)
    {
        services.Scan(scan =>
            scan.FromAssemblies(assemblies)
                .AddClasses(classes => classes.AssignableTo<IMigrationSchema>())
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );
    }
}
