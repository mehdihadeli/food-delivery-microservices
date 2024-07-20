using BuildingBlocks.Core.Domain;
using BuildingBlocks.Core.Domain.Exceptions;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Catalogs.Categories.ValueObjects;

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

    public CategoryName Name { get; private set; }
    public CategoryImage Image { get; private set; }
    public CategoryCode Code { get; private set; }
    public string? Description { get; private set; }

    public static Category Create(
        CategoryId? id,
        CategoryName? name,
        CategoryCode? code,
        CategoryImage? image,
        string? description = ""
    )
    {
        // Alexey Zimarev book: input validation will do in the `command` and our `value objects` before arriving to entity and makes or domain cleaner (but we have to check against for our value objects), here we just do business validation
        var category = new Category { Id = id.NotBeNull() };

        category.ChangeName(name);
        category.ChangeDescription(description);
        category.ChangeCode(code);
        category.ChangeImage(image);

        return category;
    }

    public void ChangeName(CategoryName? name)
    {
        // input validation will do in the command and our value objects, here we just do business validation
        name.NotBeNull(new DomainException("CategoryName can't be null."));
        Name = name;
    }

    public void ChangeCode(CategoryCode? code)
    {
        code.NotBeNull(new DomainException("CategoryCode can't be null."));
        Code = code;
    }

    public void ChangeImage(CategoryImage? image)
    {
        image.NotBeNull(new DomainException("Image can't be null."));

        Image = image;
    }

    public void ChangeDescription(string? description)
    {
        Description = description;
    }

    public override string ToString()
    {
        return $"{Name} - {Code}";
    }
}
