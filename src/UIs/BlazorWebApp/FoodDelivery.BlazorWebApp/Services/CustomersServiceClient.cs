using FoodDelivery.BlazorWebApp.Contracts;
using FoodDelivery.BlazorWebApp.Dtos;

namespace FoodDelivery.BlazorWebApp.Services;

public class CustomersServiceClient(IHttpClientFactory factory) : ICustomersServiceClient
{
    private const string CustomersBasePath = "/api-bff/api/v1/customers/customers";
    private readonly HttpClient _gatewayClient = factory.CreateClient("ApiGatewayClient");

    // GET: /api/v1/customers/customers?PageNumber=1&PageSize=10
    public async Task<GetCustomersResponse> GetCustomersAsync(
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default
    )
    {
        var url = $"{CustomersBasePath}?PageNumber={pageNumber}&PageSize={pageSize}";
        var response = await _gatewayClient.GetFromJsonAsync<GetCustomersResponse>(url, cancellationToken);
        return response!;
    }
}
