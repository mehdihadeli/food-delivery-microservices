namespace FoodDelivery.Services.Catalogs.Products.Models.Read;

public class ProductImageReadModel
{
    public required long Id { get; init; }
    public required string ImageUrl { get; init; }
    public required long ProductId { get; init; }
    public bool IsMain { get; init; }
}
