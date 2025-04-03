namespace Tests.Shared.Helpers;

public class DelegateHttpClientFactory(Func<string, Lazy<HttpClient>> httpClientProvider) : IHttpClientFactory
{
    public HttpClient CreateClient(string name)
    {
        if (name == "k8s-cluster-service" || name == "health-checks-webhooks" || name == "health-checks")
            return new HttpClient();

        return httpClientProvider(name).Value;
    }
}
