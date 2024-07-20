namespace FoodDelivery.Services.Catalogs.Suppliers.Contracts;

public interface ISupplierChecker
{
    bool SupplierExists(SupplierId supplierId);
}
