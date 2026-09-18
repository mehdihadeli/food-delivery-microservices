using CloudNativeKit.Core.Domain.Exceptions;
using CloudNativeKit.Core.Exception;

namespace FoodDelivery.Services.Catalogs.Products.Exceptions.Domain;

public class ProductDomainException : DomainException
{
    public ProductDomainException(string message)
        : base(message) { }
}
