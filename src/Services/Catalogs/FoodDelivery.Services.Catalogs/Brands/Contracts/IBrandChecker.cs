using FoodDelivery.Services.Catalogs.Brands.ValueObjects;

namespace FoodDelivery.Services.Catalogs.Brands.Contracts;

public interface IBrandChecker
{
    bool BrandExists(BrandId? brandId);
}
