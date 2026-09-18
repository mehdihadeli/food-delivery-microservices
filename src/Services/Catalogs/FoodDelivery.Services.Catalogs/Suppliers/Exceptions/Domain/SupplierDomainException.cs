using CloudNativeKit.Core.Domain.Exceptions;
using CloudNativeKit.Core.Exception;

namespace FoodDelivery.Services.Catalogs.Suppliers.Exceptions.Domain;

public class SupplierDomainException : DomainException
{
    public SupplierDomainException(string message)
        : base(message) { }
}
