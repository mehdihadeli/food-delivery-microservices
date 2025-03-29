using Bogus;
using FoodDelivery.Services.Catalogs.Brands;
using FoodDelivery.Services.Catalogs.Brands.ValueObjects;

namespace FoodDelivery.Services.Catalogs.TestShared.Fakes.Banners;

public sealed class BrandFaker : Faker<Brand>
{
    public BrandFaker()
    {
        // https://www.youtube.com/watch?v=T9pwE1GAr_U
        // https://jackhiston.com/2017/10/1/how-to-create-bogus-data-in-c/
        // https://khalidabuhakmeh.com/seed-entity-framework-core-with-bogus
        // https://github.com/bchavez/Bogus#bogus-api-support
        // https://github.com/bchavez/Bogus/blob/master/Examples/EFCoreSeedDb/Program.cs#L74
        long id = 1;
        CustomInstantiator(f => Brand.Create(BrandId.Of(id++), BrandName.Of(f.Company.CompanyName())));
    }
}
