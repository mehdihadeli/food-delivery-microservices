using BuildingBlocks.Core.Domain.Exceptions;
using BuildingBlocks.Core.Exception;

namespace FoodDelivery.Services.Catalogs.Suppliers.Exceptions.Domain;

public class SupplierDomainException : DomainException
{
    public SupplierDomainException(string message)
        : base(message) { }
}
