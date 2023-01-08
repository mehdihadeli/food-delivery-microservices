namespace ECommerce.Services.Customers.Shared.Clients.Identity.Dtos;

public record UserIdentityDto(
    Guid Id,
    string UserName,
    string Email,
    string PhoneNumber,
    string FirstName,
    string LastName);
