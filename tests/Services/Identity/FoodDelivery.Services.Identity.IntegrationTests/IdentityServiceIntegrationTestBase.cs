using FoodDelivery.Services.Identity.Api;
using FoodDelivery.Services.Identity.Shared.Data;
using Tests.Shared.Fixtures;
using Tests.Shared.TestBase;
using Xunit;

namespace FoodDelivery.Services.Identity.IntegrationTests;

//https://stackoverflow.com/questions/43082094/use-multiple-collectionfixture-on-my-test-class-in-xunit-2-x
// note: each class could have only one collection
[Collection(IntegrationTestCollection.Name)]
public class IdentityServiceIntegrationTestBase(
    SharedFixtureWithEfCore<IdentityApiMetadata, IdentityContext> sharedFixture,
    ITestOutputHelper outputHelper
) : IntegrationTestBase<IdentityApiMetadata, IdentityContext>(sharedFixture)
{
    // We don't need to inject `CustomersServiceMockServersFixture` class fixture in the constructor because it initialized by `collection fixture` and its static properties are accessible in the codes
}
