using BuildingBlocks.Core.Domain.Exceptions;
using BuildingBlocks.Core.Exception;

namespace FoodDelivery.Services.Catalogs.Products.Exceptions.Domain;

public class ProductDomainException : DomainException
{
    public ProductDomainException(string message)
        : base(message) { }
}
