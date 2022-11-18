namespace ECommerce.Services.Catalogs.Products.Features.CreatingProduct.v1.Requests;

public record CreateProductImageRequest
{
    public string ImageUrl { get; init; } = default!;
    public bool IsMain { get; init; }
}
