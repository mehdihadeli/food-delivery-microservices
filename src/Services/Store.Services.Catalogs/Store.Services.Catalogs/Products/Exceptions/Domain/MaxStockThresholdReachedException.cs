using BuildingBlocks.Core.Domain.Exceptions;

namespace Store.Services.Catalogs.Products.Exceptions.Domain;

public class MaxStockThresholdReachedException : DomainException
{
    public MaxStockThresholdReachedException(string message) : base(message)
    {
    }
}
