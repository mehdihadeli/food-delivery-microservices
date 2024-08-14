using AutoMapper;
using FoodDelivery.Services.Catalogs.Shared.Data;
using FoodDelivery.Services.Customers.UnitTests;
using Tests.Shared.XunitCategories;

namespace FoodDelivery.Services.Catalogs.UnitTests.Common;

[CollectionDefinition(nameof(QueryTestCollection))]
public class QueryTestCollection : ICollectionFixture<CatalogsServiceUnitTestBase> { }

//https://stackoverflow.com/questions/43082094/use-multiple-collectionfixture-on-my-test-class-in-xunit-2-x
// note: each class could have only one collection
[Collection(UnitTestCollection.Name)]
[CategoryTrait(TestCategory.Unit)]
public class CatalogsServiceUnitTestBase : IAsyncDisposable
{
    public CatalogsServiceUnitTestBase()
    {
        Mapper = MapperFactory.Create();
        CatalogDbContext = DbContextFactory.Create();
    }

    public IMapper Mapper { get; }
    public CatalogDbContext CatalogDbContext { get; }

    public async ValueTask DisposeAsync()
    {
        await DbContextFactory.Destroy(CatalogDbContext);
    }
}
