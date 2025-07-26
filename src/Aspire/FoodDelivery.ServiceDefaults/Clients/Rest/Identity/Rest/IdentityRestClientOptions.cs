using BuildingBlocks.Resiliency.Options;

namespace FoodDelivery.ServiceDefaults.Clients.Rest.Identity.Rest;

public class IdentityRestClientOptions : HttpClientOptions
{
    public string CreateUserEndpoint { get; set; } = default!;
    public string GetUserByEmailEndpoint { get; set; } = default!;
}
