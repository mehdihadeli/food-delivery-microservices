using FluentAssertions;
using FoodDelivery.Services.Customers.TestShared.Fakes.RestockSubscriptions.Entities;
using FoodDelivery.Services.Customers.TestShared.Fakes.RestockSubscriptions.ValueObjects;
using Tests.Shared.XunitCategories;

namespace FoodDelivery.Services.Customers.TestShared.Fakes.RestockSubscriptions;

public class Tests
{
    public class Entities
    {
        [Fact]
        [CategoryTrait(TestCategory.Unit)]
        public void fake_restocksubscription_test()
        {
            var restockSubscriptions = new FakeRestockSubscriptions().Generate(5);
            restockSubscriptions.All(x => x.Id > 0).Should().BeTrue();
            restockSubscriptions.All(x => x.CustomerId > 0).Should().BeTrue();
            restockSubscriptions.All(x => !string.IsNullOrWhiteSpace(x.Email)).Should().BeTrue();
            restockSubscriptions.All(x => !string.IsNullOrWhiteSpace(x.ProductInformation.Name)).Should().BeTrue();
            restockSubscriptions.All(x => x.ProductInformation.Id.Value > 0).Should().BeTrue();
        }
    }

    public class ValueObjects
    {
        [Fact]
        [CategoryTrait(TestCategory.Unit)]
        public void fake_product_information_test()
        {
            var productInformation = new FakeProductInformation().Generate();
            productInformation.Id.Should().NotBeNull();
            productInformation.Id.Value.Should().BeGreaterThan(0);
            productInformation.Name.Should().NotBeNullOrEmpty();
        }
    }
}
