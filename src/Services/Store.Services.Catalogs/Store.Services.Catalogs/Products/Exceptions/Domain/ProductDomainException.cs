using BuildingBlocks.Core.Domain.Exceptions;

namespace Store.Services.Catalogs.Products.Exceptions.Domain;

public class ProductDomainException : DomainException
{
    public ProductDomainException(string message) : base(message)
    {
    }
}
