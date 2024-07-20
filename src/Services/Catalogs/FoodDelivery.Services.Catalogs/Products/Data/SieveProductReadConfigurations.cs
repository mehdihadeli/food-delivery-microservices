using FoodDelivery.Services.Catalogs.Products.Models.Read;
using Sieve.Services;

namespace FoodDelivery.Services.Catalogs.Products.Data;

public class SieveProductReadConfigurations : ISieveConfiguration
{
    public void Configure(SievePropertyMapper mapper)
    {
        mapper.Property<ProductReadModel>(p => p.Id).CanFilter().CanSort().HasName("id");
        mapper.Property<ProductReadModel>(p => p.Name).CanFilter().CanSort().HasName("name");
        mapper.Property<ProductReadModel>(p => p.CategoryId).CanSort().CanFilter().HasName("categoryId");
        mapper.Property<ProductReadModel>(p => p.Price).CanFilter().HasName("price");
    }
}
