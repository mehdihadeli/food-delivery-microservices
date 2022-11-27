namespace ECommerce.Services.Customers.Shared.Clients.Identity;

public class IdentityApiClientOptions
{
    public string BaseApiAddress { get; set; } = default!;
    public string UsersEndpoint { get; set; } = default!;
}
