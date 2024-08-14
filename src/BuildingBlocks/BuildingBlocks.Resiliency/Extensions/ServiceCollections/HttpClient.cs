namespace BuildingBlocks.Resiliency.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddHttpApiClient<TInterface, TClient>(
        this IServiceCollection services,
        Action<IServiceProvider, HttpClient> configureClient
    )
        where TInterface : class
        where TClient : class, TInterface
    {
        services.AddHttpClient<TInterface, TClient>(configureClient).AddCustomPolicyHandlers();

        return services;
    }
}
