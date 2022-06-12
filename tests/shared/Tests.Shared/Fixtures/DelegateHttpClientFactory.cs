namespace Tests.Shared.Fixtures;

public class DelegateHttpClientFactory : IHttpClientFactory
{
    private readonly Func<string, Lazy<HttpClient>> _httpClientProvider;

    public DelegateHttpClientFactory(Func<string, Lazy<HttpClient>> httpClientProvider)
    {
        _httpClientProvider = httpClientProvider;
    }

    public HttpClient CreateClient(string name)
    {
        if (name == "k8s-cluster-service" || name == "health-checks-webhooks" || name == "health-checks")
            return new HttpClient();

        return _httpClientProvider(name).Value;
    }
}
