using BuildingBlocks.Core.Exception;

namespace FoodDelivery.Services.Catalogs.Products.Exceptions.Application;

public class ProductNotFoundException : AppException
{
    public ProductNotFoundException(long id)
        : base($"Product with id '{id}' not found", StatusCodes.Status404NotFound) { }

    public ProductNotFoundException(string message)
        : base(message, StatusCodes.Status404NotFound) { }
}
