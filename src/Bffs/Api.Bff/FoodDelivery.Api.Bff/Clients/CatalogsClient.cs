using FoodDelivery.WebApp.Bff.Contracts;

namespace FoodDelivery.WebApp.Bff.Clients;

public class CatalogsClient(HttpClient httpClient) : ICatalogsClient { }
