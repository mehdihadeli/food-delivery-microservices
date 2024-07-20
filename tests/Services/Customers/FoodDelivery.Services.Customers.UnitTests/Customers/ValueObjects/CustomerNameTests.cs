using Bogus;
using FoodDelivery.Services.Customers.Customers.Exceptions.Domain;
using FoodDelivery.Services.Customers.Customers.ValueObjects;
using FluentAssertions;
using Tests.Shared.XunitCategories;

namespace FoodDelivery.Services.Customers.UnitTests.Customers.ValueObjects;

public class CustomerNameTests
{
    private readonly Faker _faker;

    public CustomerNameTests()
    {
        _faker = new Faker();
    }

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
    public void must_throw_exception_with_invalid_name()
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
}
