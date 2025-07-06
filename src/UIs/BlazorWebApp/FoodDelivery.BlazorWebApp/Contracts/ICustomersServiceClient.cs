using FoodDelivery.BlazorWebApp.Dtos;

namespace FoodDelivery.BlazorWebApp.Contracts;

public interface ICustomersServiceClient
{
    Task<GetCustomersResponse> GetCustomersAsync(
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default
    );
}
