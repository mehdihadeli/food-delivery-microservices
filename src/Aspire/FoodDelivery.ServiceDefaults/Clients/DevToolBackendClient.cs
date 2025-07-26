using FoodDelivery.ServiceDefaults.Clients.Rest.Catalogs;
using FoodDelivery.ServiceDefaults.Clients.Rest.Catalogs.Rest;
using IdentityModel.Client;
using Microsoft.Extensions.Options;
using Polly.Wrap;

namespace FoodDelivery.ServiceDefaults.Clients;

public static class DevToolBackendClient
{
    /// <summary>
    /// Returns a <see cref="CatalogsRestClient"/> that is pre-authenticated for use in development and testing tools.
    /// Do not use this in application code.
    /// </summary>
    public static async Task<ICatalogsRestClient> GetDevToolCatalogsBackendClientAsync(
        HttpClient identityServerHttpClient,
        HttpClient backendHttpClient,
        IOptions<CatalogsRestClientOptions> options,
        AsyncPolicyWrap combinedPolicy
    )
    {
        var identityServerDisco = await identityServerHttpClient.GetDiscoveryDocumentAsync();
        if (identityServerDisco.IsError)
        {
            throw new InvalidOperationException(identityServerDisco.Error);
        }

        var tokenResponse = await identityServerHttpClient.RequestClientCredentialsTokenAsync(
            new ClientCredentialsTokenRequest
            {
                Address = identityServerDisco.TokenEndpoint,
                ClientId = "dev-and-test-tools",
                ClientSecret = "dev-and-test-tools-secret",
                Scope = "staff-api",
            }
        );

        backendHttpClient.SetBearerToken(tokenResponse.AccessToken!);
        return new CatalogsRestClient(backendHttpClient, options, combinedPolicy);
    }
}
