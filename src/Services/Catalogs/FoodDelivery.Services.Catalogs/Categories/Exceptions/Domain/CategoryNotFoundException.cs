using BuildingBlocks.Core.Exception.Types;

namespace FoodDelivery.Services.Catalogs.Categories.Exceptions.Domain;

public class CategoryNotFoundException : NotFoundDomainException
{
    public CategoryNotFoundException(Type businessRuleType, long id)
        : base(businessRuleType, $"Category with id '{id}' not found.") { }
}
