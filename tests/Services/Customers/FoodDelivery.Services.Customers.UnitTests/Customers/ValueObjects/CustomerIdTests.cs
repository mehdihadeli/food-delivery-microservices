using Bogus;
using FluentAssertions;
using FoodDelivery.Services.Customers.Customers.Models.ValueObjects;
using Tests.Shared.XunitCategories;
using ValidationException = BuildingBlocks.Core.Exception.ValidationException;

namespace FoodDelivery.Services.Customers.UnitTests.Customers.ValueObjects;

public class CustomerIdTests
{
    private readonly Faker _faker;

    public CustomerIdTests()
    {
        _faker = new Faker();
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void can_create_with_valid_id()
    {
        var validId = _faker.Random.Int(1, 100);
        var customerIdValueObject = CustomerId.Of(validId);
        customerIdValueObject.Value.Should().Be(validId);
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void must_throw_exception_with_invalid_id()
    {
        var invalidId = _faker.Random.Int(-100, 0);

        //Act
        var act = () =>
        {
            var customerIdValueObject = CustomerId.Of(invalidId);
            return customerIdValueObject;
        };

        // Assert
        act.Should().Throw<ValidationException>();
    }
}
