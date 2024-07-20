using BuildingBlocks.Core.Domain;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Catalogs.Brands.ValueObjects;

namespace FoodDelivery.Services.Catalogs.Brands;

public class Brand : Aggregate<BrandId>
{
    public BrandName Name { get; private set; } = default!;

    public static Brand Of(BrandId id, BrandName name)
    {
        // input validation will do in the `command` and our `value objects` before arriving to entity and makes or domain cleaner (but we have to check against for our value objects), here we just do business validation
        id.NotBeNull();
        name.NotBeNull();

        var brand = new Brand { Id = id, };

        brand.ChangeName(name);

        return brand;
    }

    public void ChangeName(BrandName name)
    {
        Name = name;
    }
}
