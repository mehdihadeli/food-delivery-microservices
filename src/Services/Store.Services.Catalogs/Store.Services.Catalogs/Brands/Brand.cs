using Ardalis.GuardClauses;
using BuildingBlocks.Core.Domain;
using Store.Services.Catalogs.Brands.Exceptions.Domain;

namespace Store.Services.Catalogs.Brands;

public class Brand : Aggregate<BrandId>
{
    public string Name { get; private set; } = null!;

    public static Brand Create(BrandId id, string name)
    {
        var brand = new Brand { Id = Guard.Against.Null(id, nameof(id)) };

        brand.ChangeName(name);

        return brand;
    }

    public void ChangeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BrandDomainException("Name can't be white space or null.");

        Name = name;
    }
}
