using BuildingBlocks.Core.Exception.Types;

namespace FoodDelivery.Services.Customers.Customers.Exceptions.Domain;

public class InvalidNationalityException(string nationality)
    : BadRequestException($"Nationality: '{nationality}' is invalid.")
{
    public string Nationality { get; } = nationality;
}
