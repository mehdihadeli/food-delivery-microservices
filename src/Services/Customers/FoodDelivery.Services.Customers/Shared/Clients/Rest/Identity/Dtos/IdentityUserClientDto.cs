namespace FoodDelivery.Services.Customers.Shared.Clients.Rest.Identity.Dtos;

public record IdentityUserClientDto(
    Guid Id,
    string UserName,
    string Email,
    string PhoneNumber,
    string FirstName,
    string LastName
);
