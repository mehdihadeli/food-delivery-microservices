using BuildingBlocks.Core.Domain.Exceptions;

namespace ECommerce.Services.Catalogs.Brands.Exceptions.Domain;

public class BrandDomainException : DomainException
{
    public BrandDomainException(string message) : base(message)
    {
    }
}
