namespace FoodDelivery.Services.Customers.UnitTests;

// https://stackoverflow.com/questions/43082094/use-multiple-collectionfixture-on-my-test-class-in-xunit-2-x
// note: each class could have only one collection, but it can implements multiple ICollectionFixture in its definitions
[CollectionDefinition(Name)]
public class UnitTestCollection
{
    public const string Name = "UnitTest Test";
}
