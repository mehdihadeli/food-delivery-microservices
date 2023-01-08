using System.Net;
using BuildingBlocks.Core.Exception.Types;

namespace ECommerce.Services.Catalogs.Products.Exceptions.Application;

public class ProductNotFoundException : AppException
{
    public ProductNotFoundException(long id) : base($"Product with id '{id}' not found", HttpStatusCode.NotFound)
    {
    }

    public ProductNotFoundException(string message) : base(message, HttpStatusCode.NotFound)
    {
    }
}
