using System.Reflection;
using BuildingBlocks.Resiliency.Fallback;
using BuildingBlocks.Resiliency.Retry;
using MediatR;
using Scrutor;

namespace BuildingBlocks.Resiliency.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediaterRetryPolicy(
        IServiceCollection services,
        IReadOnlyList<Assembly> assemblies)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RetryBehavior<,>));

        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(classes => classes.AssignableTo(typeof(IRetryableRequest<,>)))
            .UsingRegistrationStrategy(RegistrationStrategy.Skip)
            .AsImplementedInterfaces()
            .WithTransientLifetime());

        return services;
    }

    public static IServiceCollection AddMediaterFallbackPolicy(
        IServiceCollection services,
        IReadOnlyList<Assembly> assemblies)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(FallbackBehavior<,>));

        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(classes => classes.AssignableTo(typeof(IFallbackHandler<,>)))
            .UsingRegistrationStrategy(RegistrationStrategy.Skip)
            .AsImplementedInterfaces()
            .WithTransientLifetime());

        return services;
    }

    public static IServiceCollection AddHttpApiClient<TInterface, TClient>(
        this IServiceCollection services,
        Action<IServiceProvider, HttpClient> configureClient)
        where TInterface : class
        where TClient : class, TInterface
    {
        services.AddHttpClient<TInterface, TClient>(configureClient)
            .AddCustomPolicyHandlers();

        return services;
    }
}
