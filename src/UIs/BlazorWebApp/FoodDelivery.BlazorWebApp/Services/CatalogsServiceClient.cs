using FoodDelivery.BlazorWebApp.Contracts;
using FoodDelivery.BlazorWebApp.Dtos;

namespace FoodDelivery.BlazorWebApp.Services;

public class CatalogsServiceClient(IHttpClientFactory factory) : ICatalogsServiceClient
{
    private const string ProductsV1Base = "/api-bff/catalogs/api/v1/products";
    private readonly HttpClient _apiGatewayClient = factory.CreateClient("ApiGatewayClient");

    // GET: catalogs/api/v1/products?PageNumber=1&PageSize=10
    public async Task<GetProductsResponse> GetProductsAsync(
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default
    )
    {
        var url = $"{ProductsV1Base}?PageNumber={pageNumber}&PageSize={pageSize}";
        var response = await _apiGatewayClient.GetFromJsonAsync<GetProductsResponse>(url, cancellationToken);
        return response!;
    }

    // GET: /api/v1/catalogs/products/{id}
    public async Task<GetProductByIdResponse> GetProductByIdAsync(
        long id,
        CancellationToken cancellationToken = default
    )
    {
        var url = $"{ProductsV1Base}/{id}";
        var response = await _apiGatewayClient.GetFromJsonAsync<GetProductByIdResponse>(url, cancellationToken);
        return response!;
    }

    // POST: /api/v1/catalogs/products
    public async Task<ProductDto> CreateProductAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _apiGatewayClient.PostAsJsonAsync(ProductsV1Base, request, cancellationToken);
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
        var url = $"{ProductsV1Base}/{id}";
        var response = await _apiGatewayClient.PutAsJsonAsync(url, request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var product = await response.Content.ReadFromJsonAsync<ProductDto>(cancellationToken: cancellationToken);
        return product!;
    }

    // DELETE: /api/v1/catalogs/products/{id}
    public async Task DeleteProductAsync(long id, CancellationToken cancellationToken = default)
    {
        var url = $"{ProductsV1Base}/{id}";
        var response = await _apiGatewayClient.DeleteAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
