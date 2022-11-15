using BuildingBlocks.Core.Exception.Types;

namespace ECommerce.Services.Customers.Products.Exceptions;

public class ProductCustomNotFoundException : CustomNotFoundException
{
    public ProductCustomNotFoundException(long id) : base($"Product with id {id} not found")
    {
    }
}
