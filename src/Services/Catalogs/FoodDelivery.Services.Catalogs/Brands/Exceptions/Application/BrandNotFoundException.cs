using BuildingBlocks.Core.Exception;

namespace FoodDelivery.Services.Catalogs.Brands.Exceptions.Application;

public class BrandNotFoundException : NotFoundAppException
{
    public BrandNotFoundException(long id)
        : base($"Brand with id '{id}' not found") { }

    public BrandNotFoundException(string message)
        : base(message) { }
}
