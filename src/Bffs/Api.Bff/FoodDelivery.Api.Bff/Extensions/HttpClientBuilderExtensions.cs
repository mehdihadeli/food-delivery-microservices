namespace FoodDelivery.WebApp.Bff.Extensions;

public static class HttpClientBuilderExtensions
{
    public static IHttpClientBuilder AddHttpClientAuthorization(this IHttpClientBuilder builder)
    {
        return builder.AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();
    }
}
