using System.Diagnostics.CodeAnalysis;
using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Catalogs.Brands.Contracts;
using FoodDelivery.Services.Catalogs.Brands.Exceptions.Domain;
using FoodDelivery.Services.Catalogs.Brands.ValueObjects;

namespace FoodDelivery.Services.Catalogs.Products.Rules;

public class BrandIdShouldExistRuleWithExceptionType : IBusinessRuleWithExceptionType<BrandNotFoundException>
{
    private readonly IBrandChecker? _brandChecker;
    private readonly BrandId? _id;

    public BrandIdShouldExistRuleWithExceptionType([NotNull] IBrandChecker? brandChecker, [NotNull] BrandId? id)
    {
        _brandChecker = brandChecker;
        _id = id;
    }

    public bool IsBroken()
    {
        _brandChecker.NotBeNull();
        _id.NotBeNull();
        var exists = _brandChecker.BrandExists(_id);

        return !exists;
    }

    public BrandNotFoundException Exception => new(GetType(), _id!);
}
