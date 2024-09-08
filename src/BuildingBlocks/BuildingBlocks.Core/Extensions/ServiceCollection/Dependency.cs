using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Core.Extensions.ServiceCollection;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a new transient registration to the service collection only when no existing registration of the same service type and implementation type exists.
    /// In contrast to TryTryAddTransient, which only checks the service type.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="serviceType"></param>
    /// <param name="implementationType"></param>
    public static void TryTryAddTransientExact(
        this IServiceCollection services,
        Type serviceType,
        Type implementationType
    )
    {
        if (services.Any(reg => reg.ServiceType == serviceType && reg.ImplementationType == implementationType))
        {
            return;
        }

        services.TryAddTransient(serviceType, implementationType);
    }

    /// <summary>
    /// Adds a new scope registration to the service collection only when no existing registration of the same service type and implementation type exists.
    /// In contrast to TryAddScope, which only checks the service type.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="serviceType"></param>
    /// <param name="implementationType"></param>
    public static void TryAddScopeExact(this IServiceCollection services, Type serviceType, Type implementationType)
    {
        if (services.Any(reg => reg.ServiceType == serviceType && reg.ImplementationType == implementationType))
        {
            return;
        }

        services.TryAddScoped(serviceType, implementationType);
    }

    public static IServiceCollection Add<TService, TImplementation>(
        this IServiceCollection services,
        Func<IServiceProvider, TImplementation> implementationFactory,
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient
    )
        where TService : class
        where TImplementation : class, TService
    {
        switch (serviceLifetime)
        {
            case ServiceLifetime.Singleton:
                services.TryAddSingleton(implementationFactory);
                return services;

            case ServiceLifetime.Scoped:
                services.TryAddScoped(implementationFactory);
                return services;

            case ServiceLifetime.Transient:
                services.TryAddTransient(implementationFactory);
                return services;

            default:
                throw new ArgumentNullException(nameof(serviceLifetime));
        }
    }

    public static IServiceCollection Add<TService>(
        this IServiceCollection services,
        Func<IServiceProvider, TService> implementationFactory,
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient
    )
        where TService : class
    {
        switch (serviceLifetime)
        {
            case ServiceLifetime.Singleton:
                services.TryAddSingleton(implementationFactory);
                return services;

            case ServiceLifetime.Scoped:
                services.TryAddScoped(implementationFactory);
                return services;

            case ServiceLifetime.Transient:
                services.TryAddTransient(implementationFactory);
                return services;

            default:
                throw new ArgumentNullException(nameof(serviceLifetime));
        }
    }

    public static IServiceCollection Add<TService>(
        this IServiceCollection services,
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient
    )
        where TService : class
    {
        switch (serviceLifetime)
        {
            case ServiceLifetime.Singleton:
                services.TryAddSingleton<TService>();
                return services;

            case ServiceLifetime.Scoped:
                services.TryAddScoped<TService>();
                return services;

            case ServiceLifetime.Transient:
                services.TryAddTransient<TService>();
                return services;

            default:
                throw new ArgumentNullException(nameof(serviceLifetime));
        }
    }

    public static IServiceCollection Add(
        this IServiceCollection services,
        Type serviceType,
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient
    )
    {
        switch (serviceLifetime)
        {
            case ServiceLifetime.Singleton:
                services.TryAddSingleton(serviceType);
                return services;

            case ServiceLifetime.Scoped:
                services.TryAddScoped(serviceType);
                return services;

            case ServiceLifetime.Transient:
                services.TryAddTransient(serviceType);
                return services;

            default:
                throw new ArgumentNullException(nameof(serviceLifetime));
        }
    }

    public static IServiceCollection Add<TService, TImplementation>(
        this IServiceCollection services,
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient
    )
        where TService : class
        where TImplementation : class, TService
    {
        switch (serviceLifetime)
        {
            case ServiceLifetime.Singleton:
                return services.AddSingleton<TService, TImplementation>();

            case ServiceLifetime.Scoped:
                return services.AddScoped<TService, TImplementation>();

            case ServiceLifetime.Transient:
                return services.AddTransient<TService, TImplementation>();

            default:
                throw new ArgumentNullException(nameof(serviceLifetime));
        }
    }

    public static IServiceCollection Add(
        this IServiceCollection services,
        Type serviceType,
        Func<IServiceProvider, object> implementationFactory,
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient
    )
    {
        switch (serviceLifetime)
        {
            case ServiceLifetime.Singleton:
                return services.AddSingleton(serviceType, implementationFactory);

            case ServiceLifetime.Scoped:
                return services.AddScoped(serviceType, implementationFactory);

            case ServiceLifetime.Transient:
                return services.AddTransient(serviceType, implementationFactory);

            default:
                throw new ArgumentNullException(nameof(serviceLifetime));
        }
    }

    public static IServiceCollection Add(
        this IServiceCollection services,
        Type serviceType,
        Type implementationType,
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient
    )
    {
        switch (serviceLifetime)
        {
            case ServiceLifetime.Singleton:
                return services.AddSingleton(serviceType, implementationType);

            case ServiceLifetime.Scoped:
                return services.AddScoped(serviceType, implementationType);

            case ServiceLifetime.Transient:
                return services.AddTransient(serviceType, implementationType);

            default:
                throw new ArgumentNullException(nameof(serviceLifetime));
        }
    }

    // https://andrewlock.net/new-in-asp-net-core-3-service-provider-validation/
    // https://steven-giesel.com/blogPost/ce948083-974a-4c16-877f-246b8909fa6d
    // https://www.stevejgordon.co.uk/aspnet-core-dependency-injection-what-is-the-iserviceprovider-and-how-is-it-built
    // https://www.youtube.com/watch?v=8JkHgymp2R4

    /// <summary>
    /// Validate container dependencies.
    /// </summary>
    /// <param name="rootServiceProvider"></param>
    /// <param name="services"></param>
    /// <param name="assembliesToScan"></param>
    public static void ValidateDependencies(
        this IServiceProvider rootServiceProvider,
        IServiceCollection services,
        params Assembly[] assembliesToScan
    )
    {
        var scanAssemblies = assembliesToScan.Length != 0 ? assembliesToScan : [Assembly.GetExecutingAssembly(),];

        // for resolving scoped based dependencies without errors
        using var scope = rootServiceProvider.CreateScope();
        var sp = scope.ServiceProvider;

        foreach (var serviceDescriptor in services)
        {
            // Skip services that are not typically resolved directly or are special cases
            if (
                serviceDescriptor.ServiceType == typeof(IHostedService)
                || serviceDescriptor.ServiceType == typeof(IApplicationLifetime)
            )
            {
                continue;
            }

            try
            {
                var serviceType = serviceDescriptor.ServiceType;
                if (scanAssemblies.Contains(serviceType.Assembly))
                {
                    // Attempt to resolve the service
                    var service = sp.GetService(serviceType);

                    // Assert: Check that the service was resolved if it's not meant to be optional
                    if (
                        serviceDescriptor.ImplementationInstance == null
                        && serviceDescriptor.ImplementationFactory == null
                    )
                    {
                        service.NotBeNull();
                    }
                }
            }
            catch (System.Exception ex)
            {
                throw new($"Failed to resolve service {serviceDescriptor.ServiceType.FullName}: {ex.Message}", ex);
            }
        }
    }
}
