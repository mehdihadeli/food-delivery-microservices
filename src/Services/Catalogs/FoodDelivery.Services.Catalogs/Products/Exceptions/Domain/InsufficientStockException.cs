using CloudNativeKit.Core.Domain.Exceptions;
using CloudNativeKit.Core.Exception;

namespace FoodDelivery.Services.Catalogs.Products.Exceptions.Domain;

public class InsufficientStockException : DomainException
{
    public InsufficientStockException(string message)
        : base(message) { }
}
