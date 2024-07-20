using BuildingBlocks.Core.Exception.Types;

namespace FoodDelivery.Services.Customers.Products.Exceptions;

public class ProductNotFoundException : AppException
{
    public ProductNotFoundException(long id)
        : base($"Product with id {id} not found", StatusCodes.Status404NotFound) { }
}
