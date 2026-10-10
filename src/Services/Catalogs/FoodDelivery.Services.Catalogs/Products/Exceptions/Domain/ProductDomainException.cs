using Squirrel.Core.Domain.Exceptions;
using Squirrel.Core.Exception;

namespace FoodDelivery.Services.Catalogs.Products.Exceptions.Domain;

public class ProductDomainException : DomainException
{
    public ProductDomainException(string message)
        : base(message) { }
}
