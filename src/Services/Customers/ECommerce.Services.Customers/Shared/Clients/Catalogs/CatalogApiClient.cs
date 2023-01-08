using System.Net.Http.Json;
using Ardalis.GuardClauses;
using BuildingBlocks.Core.Extensions;
using ECommerce.Services.Customers.Shared.Clients.Catalogs.Dtos;
using Microsoft.Extensions.Options;

namespace ECommerce.Services.Customers.Shared.Clients.Catalogs;

public class CatalogApiClient : ICatalogApiClient
{
    private readonly HttpClient _httpClient;
    private readonly CatalogsApiClientOptions _options;

    public CatalogApiClient(HttpClient httpClient, IOptions<CatalogsApiClientOptions> options)
    {
        _httpClient = Guard.Against.Null(httpClient, nameof(httpClient));
        _options = options.Value;
    }

    public async Task<GetProductByIdResponse?> GetProductByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NegativeOrZero(id);

        // https://stackoverflow.com/questions/21097730/usage-of-ensuresuccessstatuscode-and-handling-of-httprequestexception-it-throws
        // https: //github.com/App-vNext/Polly#step-1--specify-the--exceptionsfaults-you-want-the-policy-to-handle
        var httpResponse = await _httpClient.GetAsync(
            $"{_options.ProductsEndpoint}/{id}",
            cancellationToken);

        // https://stackoverflow.com/questions/21097730/usage-of-ensuresuccessstatuscode-and-handling-of-httprequestexception-it-throws
        // throw HttpResponseException instead of HttpRequestException (because we want detail response exception) with corresponding status code
        await httpResponse.EnsureSuccessStatusCodeWithDetailAsync();

        return await httpResponse.Content.ReadFromJsonAsync<GetProductByIdResponse>(
            cancellationToken: cancellationToken);
    }
}
