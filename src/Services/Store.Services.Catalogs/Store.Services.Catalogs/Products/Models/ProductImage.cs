using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Core.Domain;
using Store.Services.Catalogs.Products.ValueObjects;

namespace Store.Services.Catalogs.Products.Models;

public class ProductImage : Entity<EntityId>
{
    public ProductImage(EntityId id, string imageUrl, bool isMain, ProductId productId)
    {
        SetImageUrl(imageUrl);
        SetIsMain(isMain);
        Id = Guard.Against.Null(id, nameof(id));
        ProductId = Guard.Against.Null(productId, nameof(productId));
    }

    // Just for EF
    private ProductImage() { }

    public string ImageUrl { get; private set; } = default!;
    public bool IsMain { get; private set; }
    public Product Product { get; private set; } = null!;
    public ProductId ProductId { get; private set; }

    public void SetIsMain(bool isMain) => IsMain = isMain;
    public void SetImageUrl(string url) => ImageUrl = url;
}
