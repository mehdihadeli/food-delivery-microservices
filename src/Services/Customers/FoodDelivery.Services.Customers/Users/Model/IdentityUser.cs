namespace FoodDelivery.Services.Customers.Users.Model;

public record UserIdentity(
    Guid Id,
    string UserName,
    string Email,
    string PhoneNumber,
    string FirstName,
    string LastName
);
