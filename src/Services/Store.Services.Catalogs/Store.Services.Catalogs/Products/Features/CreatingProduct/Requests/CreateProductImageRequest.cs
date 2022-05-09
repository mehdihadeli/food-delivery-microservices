namespace Store.Services.Catalogs.Products.Features.CreatingProduct.Requests;

public record CreateProductImageRequest
{
    public string ImageUrl { get; init; } = default!;
    public bool IsMain { get; init; }
}
