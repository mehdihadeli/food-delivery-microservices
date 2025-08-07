using FoodDelivery.BlazorWebApp.Contracts;
using FoodDelivery.BlazorWebApp.Dtos;

namespace FoodDelivery.BlazorWebApp.Services;

public class CustomersServiceClient(IHttpClientFactory factory) : ICustomersServiceClient
{
    private const string CustomersV1Base = "/api-bff/customers/api/v1/customers";
    private readonly HttpClient _apiGatewayClient = factory.CreateClient("ApiGatewayClient");

    // GET: /api/v1/customers/customers?PageNumber=1&PageSize=10
    public async Task<GetCustomersResponse> GetCustomersAsync(
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default
    )
    {
        var url = $"{CustomersV1Base}?PageNumber={pageNumber}&PageSize={pageSize}";
        var response = await _apiGatewayClient.GetFromJsonAsync<GetCustomersResponse>(url, cancellationToken);
        return response!;
    }
}
