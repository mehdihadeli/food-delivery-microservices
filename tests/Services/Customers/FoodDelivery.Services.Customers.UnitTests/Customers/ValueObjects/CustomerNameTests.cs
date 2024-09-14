using Bogus;
using FluentAssertions;
using FoodDelivery.Services.Customers.Customers.Exceptions.Domain;
using FoodDelivery.Services.Customers.Customers.ValueObjects;
using Tests.Shared.XunitCategories;
using ValidationException = BuildingBlocks.Core.Exception.Types.ValidationException;

namespace FoodDelivery.Services.Customers.UnitTests.Customers.ValueObjects;

public class CustomerNameTests
{
    private readonly Faker _faker = new();

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void can_create_name_with_valid_inputs()
    {
        var firstName = _faker.Name.FirstName();
        var lastName = _faker.Name.LastName();

        var customerNameValueObject = CustomerName.Of(firstName, lastName);
        customerNameValueObject.FirstName.Should().Be(firstName);
        customerNameValueObject.LastName.Should().Be(lastName);
        customerNameValueObject.FullName.Should().Be(firstName + " " + lastName);
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void must_throw_exception_with_null_last_name()
    {
        string firstName = string.Empty;
        string lastName = null!;

        //Act
        var act = () =>
        {
            var customerNameValueObject = CustomerName.Of(firstName, lastName);
            return customerNameValueObject;
        };

        // Assert
        act.Should().Throw<InvalidNameException>();
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void must_throw_exception_with_invalid_last_name_first_name()
    {
        string firstName = "ss";
        string lastName = string.Empty;

        //Act
        var act = () =>
        {
            var customerNameValueObject = CustomerName.Of(firstName, lastName);
            return customerNameValueObject;
        };

        // Assert
        act.Should().Throw<InvalidNameException>();
    }
}
