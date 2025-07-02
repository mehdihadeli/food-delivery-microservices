using BuildingBlocks.Resiliency.Options;

namespace FoodDelivery.Services.Customers.Shared.Clients.Rest.Catalogs.Rest;

public class CatalogsRestClientOptions : HttpClientOptions
{
    public string CreateProductEndpoint { get; set; } = default!;
    public string GetProductByPageEndpoint { get; set; } = default!;
    public string GetProductByIdEndpoint { get; set; } = default!;
}
