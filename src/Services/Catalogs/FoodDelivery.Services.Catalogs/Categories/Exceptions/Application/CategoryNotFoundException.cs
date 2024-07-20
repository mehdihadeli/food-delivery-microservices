using BuildingBlocks.Core.Exception.Types;

namespace FoodDelivery.Services.Catalogs.Categories.Exceptions.Application;

public class CategoryNotFoundException : NotFoundAppException
{
    public CategoryNotFoundException(long id)
        : base($"Category with id '{id}' not found.") { }

    public CategoryNotFoundException(string message)
        : base(message) { }
}
