using BuildingBlocks.Core.Domain.Exceptions;

namespace Store.Services.Catalogs.Brands.Exceptions.Domain;

public class BrandDomainException : DomainException
{
    public BrandDomainException(string message) : base(message)
    {
    }
}
