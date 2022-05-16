using System.Net.Http.Json;
using Ardalis.GuardClauses;
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
        _options = Guard.Against.Null(options.Value, nameof(options));

        _httpClient.BaseAddress = new Uri(_options.BaseApiAddress);
        _httpClient.Timeout = new TimeSpan(0, 0, 30);
        _httpClient.DefaultRequestHeaders.Clear();
    }


    public async Task<GetProductByIdResponse?> GetProductByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        Guard.Against.NegativeOrZero(id, nameof(id));

        var response = await _httpClient.GetFromJsonAsync<GetProductByIdResponse>(
            $"{_options.ProductsEndpoint}/{id}",
            cancellationToken);

        return response;
    }
}
