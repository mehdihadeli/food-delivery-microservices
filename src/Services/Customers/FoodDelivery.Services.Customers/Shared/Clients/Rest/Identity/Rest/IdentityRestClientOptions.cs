using BuildingBlocks.Resiliency.Options;

namespace FoodDelivery.Services.Customers.Shared.Clients.Rest.Identity.Rest;

public class IdentityRestClientOptions : HttpClientOptions
{
    public string CreateUserEndpoint { get; set; } = default!;
    public string GetUserByEmailEndpoint { get; set; } = default!;
}
