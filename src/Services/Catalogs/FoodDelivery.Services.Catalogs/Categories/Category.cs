using BuildingBlocks.Core.Domain;
using BuildingBlocks.Core.Domain.Exceptions;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Catalogs.Categories.ValueObjects;
using FoodDelivery.Services.Catalogs.Products.ValueObjects;

namespace FoodDelivery.Services.Catalogs.Categories;

// https://stackoverflow.com/a/32354885/581476
// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
// https://github.com/dotnet/efcore/issues/29940
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
public class Category : Aggregate<CategoryId>
{
    // EF
    // this constructor is needed when we have a parameter constructor that has some navigation property classes in the parameters and ef will skip it and try to find other constructor, here default constructor (maybe will fix .net 8)
    private Category() { }

    private Category(
        CategoryId categoryId,
        CategoryName name,
        CategoryCode code,
        CategoryImage image,
        Description? description
    )
    {
        Id = categoryId;
        Name = name;
        Code = code;
        Image = image;
        Description = description?.Value;
    }

    public CategoryName Name { get; private set; }
    public CategoryImage Image { get; private set; }
    public CategoryCode Code { get; private set; }
    public string? Description { get; private set; }

    public static Category Create(
        CategoryId id,
        CategoryName name,
        CategoryCode code,
        CategoryImage image,
        Description? description = null
    )
    {
        id.NotBeNull();
        name.NotBeNull();
        code.NotBeNull();

        // Alexey Zimarev book: input validation will do in the `command` and our `value objects` before arriving to entity and makes or domain cleaner (but we have to check against for our value objects), here we just do business validation
        var category = new Category(id, name, code, image, description);

        // category.AddDomainEvents(new CategoryCreated(categoryId, name, code, description));

        return category;
    }

    public void UpdateCategoryDetails(
        CategoryName newName,
        CategoryCode newCode,
        CategoryImage newCategoryImage,
        Description? newDescription
    )
    {
        newName.NotBeNull(new DomainException("CategoryName can't be null."));
        newCode.NotBeNull(new DomainException("CategoryCode can't be null."));
        newCategoryImage.NotBeNull(new DomainException("CategoryImage can't be null."));

        // this.AddDomainEvents(new CategoryDetailsUpdated(getId(), name, code, description));
    }

    public override string ToString()
    {
        return $"{Name} - {Code}";
    }
}
