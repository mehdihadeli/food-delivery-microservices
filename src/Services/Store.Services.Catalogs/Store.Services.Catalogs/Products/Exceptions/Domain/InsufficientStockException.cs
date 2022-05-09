using BuildingBlocks.Core.Domain.Exceptions;

namespace Store.Services.Catalogs.Products.Exceptions.Domain;

public class InsufficientStockException : DomainException
{
    public InsufficientStockException(string message) : base(message)
    {
    }
}
