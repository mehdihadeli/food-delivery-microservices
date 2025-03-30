using BuildingBlocks.Core.Exception.Types;

namespace FoodDelivery.Services.Customers.Customers.Exceptions.Domain;

public class UnsupportedNationalityException(string nationality)
    : BadRequestException($"Nationality: '{nationality}' is unsupported.")
{
    public string Nationality { get; } = nationality;
}
