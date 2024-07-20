using System.Diagnostics.CodeAnalysis;
using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Catalogs.Categories;
using FoodDelivery.Services.Catalogs.Categories.Exceptions.Domain;
using FoodDelivery.Services.Catalogs.Shared.Contracts;

namespace FoodDelivery.Services.Catalogs.Products.Rules;

public class CategoryIdShouldExistRuleWithExceptionType : IBusinessRuleWithExceptionType<CategoryNotFoundException>
{
    private readonly AggregateFuncOperation<CategoryId?, bool>? _categoryChecker;
    private readonly CategoryId? _id;

    public CategoryIdShouldExistRuleWithExceptionType(
        [NotNull] AggregateFuncOperation<CategoryId?, bool>? categoryChecker,
        [NotNull] CategoryId? id
    )
    {
        _categoryChecker = categoryChecker;
        _id = id;
    }

    public bool IsBroken()
    {
        _categoryChecker.NotBeNull();
        _id.NotBeNull();
        var exists = _categoryChecker(_id).GetAwaiter().GetResult();

        return !exists;
    }

    public CategoryNotFoundException Exception => new(GetType(), _id!);
}
