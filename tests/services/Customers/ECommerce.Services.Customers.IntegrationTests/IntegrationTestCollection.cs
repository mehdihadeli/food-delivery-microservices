using ECommerce.Services.Customers.Shared.Data;
using Tests.Shared.Fixtures;

namespace ECommerce.Services.Customers.IntegrationTests;

[CollectionDefinition(Name)]
public class
    IntegrationTestCollection : ICollectionFixture<SharedFixture<ECommerce.Services.Customers.Api.Program, CustomersDbContext,CustomersReadDbContext>>
{
    public const string Name = "Integration Test";
}
