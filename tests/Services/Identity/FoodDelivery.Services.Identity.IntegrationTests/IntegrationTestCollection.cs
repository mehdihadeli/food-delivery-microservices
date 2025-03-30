using FoodDelivery.Services.Identity.Api;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Tests.Shared.Fixtures;
using Xunit;

namespace FoodDelivery.Services.Identity.IntegrationTests;

// https://stackoverflow.com/questions/43082094/use-multiple-collectionfixture-on-my-test-class-in-xunit-2-x
// note: each class could have only one collection, but it can implement multiple ICollectionFixture in its definitions
[CollectionDefinition(Name)]
public class IntegrationTestCollection
    : ICollectionFixture<SharedFixtureWithEfCore<IdentityApiMetadata, IdentityDbContext>>
{
    public const string Name = "Integration Test";
}
