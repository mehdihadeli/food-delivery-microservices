using System.Diagnostics.CodeAnalysis;
using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Catalogs.Suppliers;
using FoodDelivery.Services.Catalogs.Suppliers.Contracts;

namespace FoodDelivery.Services.Catalogs.Products.Rules;

public class SupplierShouldExistRule : IBusinessRule
{
    private readonly ISupplierChecker? _supplierChecker;
    private readonly SupplierId? _id;

    public SupplierShouldExistRule([NotNull] ISupplierChecker? supplierChecker, [NotNull] SupplierId? id)
    {
        _supplierChecker = supplierChecker;
        _id = id;
    }

    public bool IsBroken()
    {
        _supplierChecker.NotBeNull();
        _id.NotBeNull();
        var exists = _supplierChecker.SupplierExists(_id);

        return !exists;
    }

    public string Message => $"Supplier with id {_id} not exist.";

    public int Status => StatusCodes.Status404NotFound;
}
