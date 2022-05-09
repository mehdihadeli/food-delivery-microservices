using BuildingBlocks.Core.Exception.Types;

namespace Store.Services.Catalogs.Brands.Exceptions.Application;

public class BrandNotFoundException : NotFoundException
{
    public BrandNotFoundException(long id) : base($"Brand with id '{id}' not found")
    {
    }

    public BrandNotFoundException(string message) : base(message)
    {
    }
}
