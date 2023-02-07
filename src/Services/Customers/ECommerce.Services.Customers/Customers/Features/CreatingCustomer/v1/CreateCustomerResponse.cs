namespace ECommerce.Services.Customers.Customers.Features.CreatingCustomer.v1;

public record CreateCustomerResponse(
    long CustomerId,
    Guid IdentityUserId);
