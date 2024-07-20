namespace FoodDelivery.Services.Customers.Shared.Clients.Identity.Dtos;

public record CreateUserClientDto(
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string ConfirmPassword
);
