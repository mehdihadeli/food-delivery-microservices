using BuildingBlocks.Core.Domain.Exceptions;

namespace ECommerce.Services.Catalogs.Categories.Exceptions.Domain;

public class CategoryDomainException : DomainException
{
    public CategoryDomainException(string message) : base(message)
    {
    }

    public CategoryDomainException(long id) : base($"Category with id: '{id}' not found.")
    {
    }
}
