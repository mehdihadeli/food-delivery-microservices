using BuildingBlocks.Core.Exception.Types;

namespace FoodDelivery.Services.Catalogs.Suppliers.Exceptions.Domain;

public class SupplierNotFoundException : NotFoundDomainException
{
    public SupplierNotFoundException(Type businessRuleType, long id)
        : base(businessRuleType, $"Supplier with id '{id}' not found.") { }
}
