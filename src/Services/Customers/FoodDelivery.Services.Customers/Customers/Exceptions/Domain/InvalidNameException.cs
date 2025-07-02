using BuildingBlocks.Core.Exception;

namespace FoodDelivery.Services.Customers.Customers.Exceptions.Domain;

public class InvalidNameException(string name) : BadRequestException($"Name: '{name}' is invalid.")
{
    public string Name { get; } = name;
}
