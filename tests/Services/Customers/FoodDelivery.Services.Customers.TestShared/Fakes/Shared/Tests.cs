using FluentAssertions;
using FoodDelivery.Services.Customers.TestShared.Fakes.Shared.Dtos;
using Tests.Shared.XunitCategories;

namespace FoodDelivery.Services.Customers.TestShared.Fakes.Shared;

public class Tests
{
    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void fake_user_identity_dto_test()
    {
        var userIdentityDto = new FakeUserIdentityDto().Generate(1).First();
        userIdentityDto.Id.Should().NotBeEmpty();
        userIdentityDto.UserName.Should().NotBeEmpty();
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void fake_product_dto_test()
    {
        var productDto = new FakeProductDto().Generate(1).First();
        productDto.Id.Should().BeGreaterThan(0);
        productDto.Name.Should().NotBeEmpty();
    }
}
