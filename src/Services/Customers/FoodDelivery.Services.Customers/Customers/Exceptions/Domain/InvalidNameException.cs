using BuildingBlocks.Core.Exception.Types;

namespace FoodDelivery.Services.Customers.Customers.Exceptions.Domain;

public class InvalidNameException : BadRequestException
{
    public string Name { get; }

    public InvalidNameException(string name)
        : base($"Name: '{name}' is invalid.")
    {
        Name = name;
    }
}
