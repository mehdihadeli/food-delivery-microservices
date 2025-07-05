using FoodDelivery.BlazorWebApp.Contracts;
using FoodDelivery.BlazorWebApp.Dtos;

namespace FoodDelivery.BlazorWebApp.Services;

public class CatalogsServiceClient(IHttpClientFactory factory) : ICatalogsServiceClient
{
    private const string ProductsBasePath = "/api-bff/api/v1/catalogs/products";
    private readonly HttpClient _gatewayClient = factory.CreateClient("ApiGatewayClient");

    // GET: /api/v1/catalogs/products?PageNumber=1&PageSize=10
    public async Task<GetProductsResponse> GetProductsAsync(
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default
    )
    {
        var url = $"{ProductsBasePath}?PageNumber={pageNumber}&PageSize={pageSize}";
        var response = await _gatewayClient.GetFromJsonAsync<GetProductsResponse>(url, cancellationToken);
        return response!;
    }

    // GET: /api/v1/catalogs/products/{id}
    public async Task<GetProductByIdResponse> GetProductByIdAsync(
        long id,
        CancellationToken cancellationToken = default
    )
    {
        var url = $"{ProductsBasePath}/{id}";
        var response = await _gatewayClient.GetFromJsonAsync<GetProductByIdResponse>(url, cancellationToken);
        return response!;
    }

    // POST: /api/v1/catalogs/products
    public async Task<ProductDto> CreateProductAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _gatewayClient.PostAsJsonAsync(ProductsBasePath, request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var product = await response.Content.ReadFromJsonAsync<ProductDto>(cancellationToken: cancellationToken);
        return product!;
    }

    // PUT: /api/v1/catalogs/products/{id}
    public async Task<ProductDto> UpdateProductAsync(
        long id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var url = $"{ProductsBasePath}/{id}";
        var response = await _gatewayClient.PutAsJsonAsync(url, request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var product = await response.Content.ReadFromJsonAsync<ProductDto>(cancellationToken: cancellationToken);
        return product!;
    }

    // DELETE: /api/v1/catalogs/products/{id}
    public async Task DeleteProductAsync(long id, CancellationToken cancellationToken = default)
    {
        var url = $"{ProductsBasePath}/{id}";
        var response = await _gatewayClient.DeleteAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
