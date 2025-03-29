namespace FoodDelivery.Services.Customers.Shared.Clients.Rest.Identity.Dtos;

public record CreateUserClientRequestDto(
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string ConfirmPassword
);
