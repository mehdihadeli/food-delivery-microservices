using Bogus;
using FoodDelivery.Services.Catalogs.Suppliers;

namespace FoodDelivery.Services.Catalogs.TestShared.Fakes.Suppliers;

public sealed class SupplierFaker : Faker<Supplier>
{
    public SupplierFaker()
    {
        long id = 1;

        // https://www.youtube.com/watch?v=T9pwE1GAr_U
        // https://jackhiston.com/2017/10/1/how-to-create-bogus-data-in-c/
        // https://khalidabuhakmeh.com/seed-entity-framework-core-with-bogus
        // https://github.com/bchavez/Bogus#bogus-api-support
        // https://github.com/bchavez/Bogus/blob/master/Examples/EFCoreSeedDb/Program.cs#L74

        // Call for objects that have complex initialization
        // faker doesn't work with normal syntax because it has no default constructor
        CustomInstantiator(faker =>
        {
            var supplier = new Supplier(SupplierId.Of(id++), faker.Person.FullName);
            return supplier;
        });
    }
}
