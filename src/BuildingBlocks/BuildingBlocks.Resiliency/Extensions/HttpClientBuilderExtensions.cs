namespace BuildingBlocks.Resiliency.Extensions;

public static partial class HttpClientBuilderExtensions
{
    public static IHttpClientBuilder AddCustomPolicyHandlers(
        this IHttpClientBuilder httpClientBuilder,
        Func<IHttpClientBuilder, IHttpClientBuilder>? builder = null)
    {
        var result = httpClientBuilder
            .AddRetryPolicyHandler()
            .AddCircuitBreakerHandler();

        if (builder is { })
            result = builder.Invoke(result);

        return result;
    }
}