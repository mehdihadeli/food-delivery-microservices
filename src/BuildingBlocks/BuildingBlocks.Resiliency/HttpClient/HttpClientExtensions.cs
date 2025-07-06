using BuildingBlocks.Core.Extensions.ServiceCollectionExtensions;
using BuildingBlocks.Resiliency.Options;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Resiliency.HttpClient;

public static class HttpClientExtensions
{
    public static IServiceCollection AddCustomHttpClient<TClient, TImplementation, TClientOptions>(
        this IServiceCollection services,
        Action<IServiceProvider, System.Net.Http.HttpClient>? configureClient = null
    )
        where TClient : class
        where TImplementation : class, TClient
        where TClientOptions : HttpClientOptions, new()
    {
        // https://github.com/App-vNext/Polly/wiki/Polly-and-HttpClientFactory
        // https://github.com/App-vNext/Polly/wiki/PolicyRegistry
        services.AddValidationOptions<TClientOptions>();

        services
            .AddHttpClient<TClient, TImplementation>()
            .ConfigureHttpClient(
                (serviceProvider, httpClient) =>
                {
                    var httpClientOptions = serviceProvider.GetRequiredService<IOptions<TClientOptions>>().Value;

                    var policyOptions = serviceProvider.GetRequiredService<IOptions<PolicyOptions>>().Value;

                    httpClient.BaseAddress = new Uri(httpClientOptions.BaseAddress);

                    httpClient.Timeout = TimeSpan.FromSeconds(policyOptions.TimeoutPolicyOptions.TimeoutInSeconds);

                    configureClient?.Invoke(serviceProvider, httpClient);
                }
            )
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests
            .AddHeaderPropagation();

        return services;
    }

    public static IServiceCollection AddCustomHttpClient<TClient, TClientOptions>(
        this IServiceCollection services,
        Action<IServiceProvider, System.Net.Http.HttpClient>? configureClient = null
    )
        where TClient : class
        where TClientOptions : HttpClientOptions, new()
    {
        return services.AddCustomHttpClient<TClient, TClient, TClientOptions>(configureClient);
    }

    public static IServiceCollection AddCustomHttpClient<TClientOptions>(
        this IServiceCollection services,
        string clientName,
        Action<IServiceProvider, System.Net.Http.HttpClient>? configureClient = null
    )
        where TClientOptions : HttpClientOptions, new()
    {
        // https://github.com/App-vNext/Polly/wiki/Polly-and-HttpClientFactory#step-2-configure-a-client-with-polly-policies-in-startup
        services.AddValidationOptions<TClientOptions>();

        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests#named-clients
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-7.0#createclient
        services
            .AddHttpClient(clientName)
            .ConfigureHttpClient(
                (sp, httpClient) =>
                {
                    var httpClientOptions = sp.GetRequiredService<IOptions<TClientOptions>>().Value;
                    var policyOptions = sp.GetRequiredService<IOptions<PolicyOptions>>().Value;
                    httpClient.BaseAddress = new Uri(httpClientOptions.BaseAddress);
                    httpClient.Timeout = TimeSpan.FromSeconds(policyOptions.TimeoutPolicyOptions.TimeoutInSeconds);

                    configureClient?.Invoke(sp, httpClient);
                }
            )
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests
            .AddHeaderPropagation();

        return services;
    }
}
