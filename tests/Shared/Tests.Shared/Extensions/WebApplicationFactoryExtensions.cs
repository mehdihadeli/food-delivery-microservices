using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Tests.Shared.Extensions;

public static class WebApplicationFactoryExtensions
{
    public static HttpClient CreateClientWithTestAuth<T>(this WebApplicationFactory<T> factory) where T : class
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Constants.AuthConstants.Scheme);

        return client;
    }
}
