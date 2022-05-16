using BuildingBlocks.Core.Exception.Types;

namespace ECommerce.Services.Customers.Customers.Exceptions.Domain;

public class UnsupportedNationalityException : BadRequestException
{
    public string Nationality { get; }

    public UnsupportedNationalityException(string nationality) : base($"Nationality: '{nationality}' is unsupported.")
    {
        Nationality = nationality;
    }
}
