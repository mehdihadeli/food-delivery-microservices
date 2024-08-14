using AutoMapper;
using FoodDelivery.Services.Catalogs.Products;

namespace FoodDelivery.Services.Catalogs.UnitTests.Common;

public static class MapperFactory
{
    public static IMapper Create()
    {
        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ProductMappers>();
        });

        return configurationProvider.CreateMapper();
    }
}
