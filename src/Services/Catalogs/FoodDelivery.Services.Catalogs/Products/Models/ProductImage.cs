using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Core.Domain;
using FoodDelivery.Services.Catalogs.Products.ValueObjects;

namespace FoodDelivery.Services.Catalogs.Products.Models;

public class ProductImage : Entity<EntityId>
{
    // Id will use in the url
    public ProductImage(EntityId id, string imageUrl, bool isMain, ProductId productId)
    {
        Id = id;
        ImageUrl = imageUrl;
        IsMain = isMain;
        ProductId = productId;
    }

    // Just for EF
    private ProductImage() { }

    public string ImageUrl { get; private set; } = default!;
    public bool IsMain { get; private set; }
    public Product Product { get; private set; } = default!;
    public ProductId ProductId { get; private set; } = default!;
}
