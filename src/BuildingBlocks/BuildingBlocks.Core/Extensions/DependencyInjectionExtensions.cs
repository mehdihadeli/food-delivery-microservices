using System.Reflection;
using BuildingBlocks.Abstractions.Core;
using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Abstractions.Serialization;
using BuildingBlocks.Abstractions.Types;
using BuildingBlocks.Core.Commands;
using BuildingBlocks.Core.Events.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Core.Messaging.Extensions;
using BuildingBlocks.Core.Paging;
using BuildingBlocks.Core.Persistence;
using BuildingBlocks.Core.Persistence.Extensions;
using BuildingBlocks.Core.Queries;
using BuildingBlocks.Core.Reflection;
using BuildingBlocks.Core.Serialization;
using BuildingBlocks.Core.Types;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;
using Sieve.Services;

namespace BuildingBlocks.Core.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddCore(this IServiceCollection services, params Assembly[] scanAssemblies)
    {
        // Assemblies are lazy loaded so using AppDomain.GetAssemblies is not reliable (it is possible to get ReflectionTypeLoadException, because some dependent type assembly are lazy and not loaded yet), so we use `GetAllReferencedAssemblies` and it
        // loads all referenced assemblies explicitly.
        var assemblies =
            scanAssemblies.Length != 0
                ? scanAssemblies
                : ReflectionUtilities.GetReferencedAssemblies(Assembly.GetCallingAssembly()).Distinct().ToArray();

        var systemInfo = MachineInstanceInfo.New();
        services.TryAddSingleton<IMachineInstanceInfo>(systemInfo);
        services.TryAddSingleton(systemInfo);
        services.TryAddSingleton<IExclusiveLock, ExclusiveLock>();

        services.TryAddScoped<ISieveProcessor, ApplicationSieveProcessor>();

        var policy = Policy.Handle<System.Exception>().RetryAsync(2);
        services.TryAddSingleton<AsyncPolicy>(policy);

        services.AddHttpContextAccessor();

        services.AddDefaultSerializer();

        services.AddMessagingCore(assemblies);

        services.AddCommandBus();

        services.AddQueryBus();

        services.AddEventBus(assemblies);

        services.AddPersistenceCore(assemblies);

        return services;
    }
}
