using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Catalogs.Brands.Contracts;
using FoodDelivery.Services.Catalogs.Brands.ValueObjects;
using FoodDelivery.Services.Catalogs.Shared.Contracts;
using FoodDelivery.Services.Catalogs.Shared.Extensions;

namespace FoodDelivery.Services.Catalogs.Brands.Services;

public class BrandChecker : IBrandChecker
{
    private readonly ICatalogDbContext _catalogDbContext;

    public BrandChecker(ICatalogDbContext catalogDbContext)
    {
        _catalogDbContext = catalogDbContext;
    }

    public bool BrandExists(BrandId? brandId)
    {
        brandId.NotBeNull();
        var brand = _catalogDbContext.FindBrand(brandId);

        return brand is not null;
    }
}
