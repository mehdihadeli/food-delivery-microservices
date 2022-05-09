namespace Store.Services.Customers.Shared.Clients.Identity.Dtos;

public record CreateUserRequest(
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string ConfirmPassword);
