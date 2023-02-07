using Ardalis.GuardClauses;
using BuildingBlocks.Core.Domain;
using ECommerce.Services.Catalogs.Categories.Exceptions.Domain;

namespace ECommerce.Services.Catalogs.Categories;

// https://stackoverflow.com/a/32354885/581476
// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
// https://github.com/dotnet/efcore/issues/29940
public class Category : Aggregate<CategoryId>
{
    // EF
    // this constructor is needed when we have a parameter constructor that has some navigation property classes in the parameters and ef will skip it and try to find other constructor, here default constructor (maybe will fix .net 8)
    public Category()
    {
    }

    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public string Code { get; private set; } = default!;

    public static Category Create(CategoryId id, string name, string code, string description = "")
    {
        var category = new Category {Id = Guard.Against.Null(id, nameof(id))};

        category.ChangeName(name);
        category.ChangeDescription(description);
        category.ChangeCode(code);

        return category;
    }

    public void ChangeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new CategoryDomainException("Name can't be white space or null.");

        Name = name;
    }

    public void ChangeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new CategoryDomainException("Code can't be white space or null.");

        Code = code;
    }

    public void ChangeDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new CategoryDomainException("Description can't be white space or null.");

        Description = description;
    }

    public override string ToString()
    {
        return $"{Name} - {Code}";
    }
}
