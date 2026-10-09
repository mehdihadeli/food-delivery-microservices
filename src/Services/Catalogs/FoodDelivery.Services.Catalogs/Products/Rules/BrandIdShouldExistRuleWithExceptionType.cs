using FoodDelivery.Services.Catalogs.Brands.Contracts;
using FoodDelivery.Services.Catalogs.Brands.Exceptions.Domain;
using FoodDelivery.Services.Catalogs.Brands.ValueObjects;
using Squirrel.Abstractions.Domain;
using Squirrel.Core.Extensions;

namespace FoodDelivery.Services.Catalogs.Products.Rules;

public class BrandIdShouldExistRuleWithExceptionType(IBrandChecker brandChecker, BrandId id)
    : IBusinessRuleWithExceptionType<BrandNotFoundException>
{
    public bool IsBroken()
    {
        brandChecker.NotBeNull();
        id.NotBeNull();
        var exists = brandChecker.BrandExists(id);

        return !exists;
    }

    public BrandNotFoundException Exception => new(GetType(), id!);
}
