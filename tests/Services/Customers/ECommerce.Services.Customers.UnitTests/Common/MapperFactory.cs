using AutoMapper;
using ECommerce.Services.Customers.Customers;
using ECommerce.Services.Customers.RestockSubscriptions;

namespace ECommerce.Services.Customers.UnitTests.Common;

public static class MapperFactory
{
    public static IMapper Create()
    {
        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CustomersMapping>();
            //cfg.AddProfile<RestockSubscriptionsMapping>();
        });

        return configurationProvider.CreateMapper();
    }
}
