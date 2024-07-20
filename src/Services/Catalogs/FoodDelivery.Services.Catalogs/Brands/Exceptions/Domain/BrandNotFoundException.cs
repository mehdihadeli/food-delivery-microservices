using BuildingBlocks.Core.Exception.Types;

namespace FoodDelivery.Services.Catalogs.Brands.Exceptions.Domain;

public class BrandNotFoundException : NotFoundDomainException
{
    public BrandNotFoundException(Type businessRuleType, long id)
        : base(businessRuleType, $"Brand with id '{id}' not found") { }
}
