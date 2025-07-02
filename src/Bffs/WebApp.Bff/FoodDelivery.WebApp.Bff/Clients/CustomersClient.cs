using FoodDelivery.WebApp.Bff.Contracts;

namespace FoodDelivery.WebApp.Bff.Clients;

public class CustomersClient(HttpClient httpClient) : ICustomersClient { }
