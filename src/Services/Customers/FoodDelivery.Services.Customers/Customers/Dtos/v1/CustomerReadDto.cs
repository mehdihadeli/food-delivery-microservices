namespace FoodDelivery.Services.Customers.Customers.Dtos.v1;

public record CustomerReadDto(
    Guid Id,
    long CustomerId,
    Guid IdentityId,
    string Email,
    string Name,
    DateTime CreatedAt,
    string? Country = null,
    string? City = null,
    string? DetailAddress = null,
    string? Nationality = null,
    DateTime? BirthDate = null,
    string? PhoneNumber = null
);
