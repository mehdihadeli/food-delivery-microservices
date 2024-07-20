using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Core.Domain;

namespace FoodDelivery.Services.Catalogs.Categories;

public class CategoryImage : Entity<EntityId>
{
    // Id will use in the url
    public CategoryImage(EntityId id, string imageUrl, bool isMain, CategoryId categoryId)
    {
        Id = id;
        ImageUrl = imageUrl;
        CategoryId = categoryId;
    }

    // Just for EF
    private CategoryImage() { }

    public string ImageUrl { get; private set; } = default!;
    public Category Category { get; private set; } = default!;
    public CategoryId CategoryId { get; private set; } = default!;
}
