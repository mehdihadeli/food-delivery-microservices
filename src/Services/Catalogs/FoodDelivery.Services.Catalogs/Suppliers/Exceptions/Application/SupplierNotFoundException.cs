using BuildingBlocks.Core.Exception;

namespace FoodDelivery.Services.Catalogs.Suppliers.Exceptions.Application;

public class SupplierNotFoundException : NotFoundAppException
{
    public SupplierNotFoundException(long id)
        : base($"Supplier with id '{id}' not found") { }

    public SupplierNotFoundException(string message)
        : base(message) { }
}
