using AutoMapper;
using FoodDelivery.Services.Customers.Customers;
using FoodDelivery.Services.Customers.RestockSubscriptions;

namespace FoodDelivery.Services.Customers.UnitTests.Common;

public static class MapperFactory
{
    public static IMapper Create()
    {
        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CustomersMapping>();
            cfg.AddProfile<RestockSubscriptionsMapping>();
        });

        return configurationProvider.CreateMapper();
    }
}
