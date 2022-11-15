using BuildingBlocks.Core.Exception.Types;

namespace ECommerce.Services.Catalogs.Brands.Exceptions.Application;

public class BrandCustomNotFoundException : CustomNotFoundException
{
    public BrandCustomNotFoundException(long id) : base($"Brand with id '{id}' not found")
    {
    }

    public BrandCustomNotFoundException(string message) : base(message)
    {
    }
}
