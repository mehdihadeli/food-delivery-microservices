using FoodDelivery.Spa.Bff.Contracts;

namespace FoodDelivery.Spa.Bff.Clients;

public class CustomersClient(HttpClient httpClient) : ICustomersClient { }
