using BuildingBlocks.Core.Domain;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Catalogs.Brands.ValueObjects;

namespace FoodDelivery.Services.Catalogs.Brands;

public class Brand : Aggregate<BrandId>
{
    // EF
    // this constructor is needed when we have a parameter constructor that has some navigation property classes in the parameters and ef will skip it and try to find other constructor, here default constructor (maybe will fix .net 8)
    private Brand() { }

    private Brand(BrandId brandId, BrandName name)
    {
        Id = brandId;
        Name = name;
    }

    public BrandName Name { get; private set; }

    public static Brand Create(BrandId id, BrandName name)
    {
        id.NotBeNull();
        name.NotBeNull();

        var brand = new Brand(id, name);

        // brand.AddDomainEvents(new BrandCreated(id, name));

        return brand;
    }

    public void ChangeName(BrandName newName)
    {
        var oldName = Name;
        Name = newName;

        // brand.AddDomainEvents(new BrandNameChanged(oldName, newName));
    }
}
