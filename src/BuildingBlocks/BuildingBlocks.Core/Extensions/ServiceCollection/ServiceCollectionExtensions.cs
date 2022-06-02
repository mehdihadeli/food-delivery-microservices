using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlocks.Core.Extensions.ServiceCollection;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection Replace<TService, TImplementation>(
        this IServiceCollection services,
        ServiceLifetime lifetime)
    {
        return services.Replace(new ServiceDescriptor(typeof(TService), typeof(TImplementation), lifetime));
    }

    public static IServiceCollection ReplaceScoped<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        return services.Replace(ServiceDescriptor.Scoped<TService, TImplementation>());
    }

    public static IServiceCollection ReplaceScoped<TService>(
        this IServiceCollection services,
        Func<IServiceProvider, TService> implementationFactory)
        where TService : class
    {
        return services.Replace(ServiceDescriptor.Scoped(implementationFactory));
    }

    public static IServiceCollection ReplaceTransient<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        return services.Replace(ServiceDescriptor.Transient<TService, TImplementation>());
    }

    public static IServiceCollection ReplaceTransient<TService>(
        this IServiceCollection services,
        Func<IServiceProvider, TService> implementationFactory)
        where TService : class
    {
        return services.Replace(ServiceDescriptor.Transient(implementationFactory));
    }

    public static IServiceCollection ReplaceSingleton<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        return services.Replace(ServiceDescriptor.Singleton<TService, TImplementation>());
    }

    public static IServiceCollection ReplaceSingleton<TService>(
        this IServiceCollection services,
        Func<IServiceProvider, TService> implementationFactory)
        where TService : class
    {
        return services.Replace(ServiceDescriptor.Singleton(implementationFactory));
    }

    public static IServiceCollection Add<TService, TImplementation>(
        this IServiceCollection services,
        Func<IServiceProvider, TImplementation> implementationFactory,
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TService : class
        where TImplementation : class, TService
    {
        switch (serviceLifetime)
        {
            case ServiceLifetime.Singleton:
                return services.AddSingleton<TService, TImplementation>(implementationFactory);

            case ServiceLifetime.Scoped:
                return services.AddScoped<TService, TImplementation>(implementationFactory);

            case ServiceLifetime.Transient:
                return services.AddTransient<TService, TImplementation>(implementationFactory);

            default:
                throw new ArgumentNullException(nameof(serviceLifetime));
        }
    }

    public static IServiceCollection Add<TService>(
        this IServiceCollection services,
        Func<IServiceProvider, TService> implementationFactory,
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TService : class
    {
        switch (serviceLifetime)
        {
            case ServiceLifetime.Singleton:
                return services.AddSingleton(implementationFactory);

            case ServiceLifetime.Scoped:
                return services.AddScoped(implementationFactory);

            case ServiceLifetime.Transient:
                return services.AddTransient(implementationFactory);

            default:
                throw new ArgumentNullException(nameof(serviceLifetime));
        }
    }

    public static IServiceCollection Add<TService>(
        this IServiceCollection services,
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TService : class
    {
        switch (serviceLifetime)
        {
            case ServiceLifetime.Singleton:
                return services.AddSingleton<TService>();

            case ServiceLifetime.Scoped:
                return services.AddScoped<TService>();

            case ServiceLifetime.Transient:
                return services.AddTransient<TService>();

            default:
                throw new ArgumentNullException(nameof(serviceLifetime));
        }
    }

    public static IServiceCollection Add(
        this IServiceCollection services,
        Type serviceType,
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        switch (serviceLifetime)
        {
            case ServiceLifetime.Singleton:
                return services.AddSingleton(serviceType);

            case ServiceLifetime.Scoped:
                return services.AddScoped(serviceType);

            case ServiceLifetime.Transient:
                return services.AddTransient(serviceType);

            default:
                throw new ArgumentNullException(nameof(serviceLifetime));
        }
    }

    public static IServiceCollection Add<TService, TImplementation>(
        this IServiceCollection services,
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
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
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
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
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
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
}
