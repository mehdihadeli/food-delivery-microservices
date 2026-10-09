using Squirrel.Core.Domain.Exceptions;
using Squirrel.Core.Exception;

namespace FoodDelivery.Services.Catalogs.Products.Exceptions.Domain;

public class MaxStockThresholdReachedException : DomainException
{
    public MaxStockThresholdReachedException(string message)
        : base(message) { }
}
