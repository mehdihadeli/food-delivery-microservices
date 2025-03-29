using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Catalogs.Categories;
using FoodDelivery.Services.Catalogs.Categories.Contracts;
using FoodDelivery.Services.Catalogs.Categories.Exceptions.Domain;

namespace FoodDelivery.Services.Catalogs.Products.Rules;

public class CategoryIdShouldExistRuleWithExceptionType(ICategoryChecker categoryChecker, CategoryId id)
    : IBusinessRuleWithExceptionType<CategoryNotFoundException>
{
    public bool IsBroken()
    {
        categoryChecker.NotBeNull();
        id.NotBeNull();
        var exists = categoryChecker.CategoryExists(id);

        return !exists;
    }

    public CategoryNotFoundException Exception => new(GetType(), id!);
}
