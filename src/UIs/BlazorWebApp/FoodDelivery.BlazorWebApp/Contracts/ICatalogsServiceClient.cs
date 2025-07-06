using FoodDelivery.BlazorWebApp.Dtos;

namespace FoodDelivery.BlazorWebApp.Contracts;

public interface ICatalogsServiceClient
{
    Task<GetProductsResponse> GetProductsAsync(
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default
    );

    Task<GetProductByIdResponse> GetProductByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<ProductDto> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default);

    Task<ProductDto> UpdateProductAsync(
        long id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default
    );

    Task DeleteProductAsync(long id, CancellationToken cancellationToken = default);
}
