using Squirrel.Core.Domain.Exceptions;
using Squirrel.Core.Exception;

namespace FoodDelivery.Services.Catalogs.Products.Exceptions.Domain;

public class InsufficientStockException : DomainException
{
    public InsufficientStockException(string message)
        : base(message) { }
}
