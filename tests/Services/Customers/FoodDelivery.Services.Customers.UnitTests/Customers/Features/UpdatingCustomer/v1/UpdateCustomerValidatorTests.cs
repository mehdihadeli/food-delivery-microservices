using Bogus;
using FluentAssertions;
using FluentValidation.TestHelper;
using FoodDelivery.Services.Customers.Customers.Features.UpdatingCustomer.v1;
using FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Commands;
using FoodDelivery.Services.Customers.UnitTests.Common;
using Tests.Shared.XunitCategories;

namespace FoodDelivery.Services.Customers.UnitTests.Customers.Features.UpdatingCustomer.v1;

public class UpdateCustomerValidatorTests : CustomerServiceUnitTestBase
{
    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void must_success_with_valid_inputs()
    {
        var updateCommand = new FakeUpdateCustomer(1).Generate();
        var validator = new UpdateCustomerValidator();

        var result = validator.TestValidate(updateCommand);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void must_success_with_valid_phone_number()
    {
        var updateCommand = new UpdateCustomer(
            0,
            string.Empty,
            string.Empty,
            string.Empty,
            new Faker().Phone.PhoneNumber("(+##)##########")
        );

        var validator = new UpdateCustomerValidator();

        var result = validator.TestValidate(updateCommand);
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void must_fail_with_invalid_min_lenght_phone_number()
    {
        var updateCommand = new UpdateCustomer(0, string.Empty, string.Empty, string.Empty, "1235");

        var validator = new UpdateCustomerValidator();

        var result = validator.TestValidate(updateCommand);
        result
            .ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithErrorMessage("PhoneNumber must not be less than 7 characters.");
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void must_fail_with_invalid_max_lenght_phone_number()
    {
        var updateCommand = new UpdateCustomer(
            0,
            string.Empty,
            string.Empty,
            string.Empty,
            "123555555555555555555555555555"
        );

        var validator = new UpdateCustomerValidator();

        var result = validator.TestValidate(updateCommand);
        result
            .ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithErrorMessage("PhoneNumber must not exceed 15 characters.");
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void must_fail_with_empty_phone_number()
    {
        var updateCommand = new UpdateCustomer(0, string.Empty, string.Empty, string.Empty, string.Empty);

        var validator = new UpdateCustomerValidator();

        var result = validator.TestValidate(updateCommand);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber).WithErrorMessage("Phone Number is required.");
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void must_success_with_valid_email_input()
    {
        var updateCommand = new UpdateCustomer(0, string.Empty, string.Empty, "test@emaple.com", string.Empty);
        var validator = new UpdateCustomerValidator();

        var result = validator.TestValidate(updateCommand);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void must_fail_with_null_or_empty_email()
    {
        // Arrange
        var updateCommand = new UpdateCustomer(0, string.Empty, string.Empty, string.Empty, string.Empty);
        var validator = new UpdateCustomerValidator();

        var result = validator.TestValidate(updateCommand);
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void must_fail_with_invalid_email()
    {
        // Arrange
        var updateCommand = new UpdateCustomer(0, string.Empty, string.Empty, "invalid_email", string.Empty);
        var validator = new UpdateCustomerValidator();

        var result = validator.TestValidate(updateCommand);
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("Email address is invalid.");
    }

    [Fact]
    public void must_fail_with_null_or_empty_firstname()
    {
        // Arrange
        var updateCommand = new UpdateCustomer(0, string.Empty, "stone", string.Empty, string.Empty);
        var validator = new UpdateCustomerValidator();

        var result = validator.TestValidate(updateCommand);
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void must_success_with_valid_firstname()
    {
        // Arrange
        var updateCommand = new UpdateCustomer(0, "john", string.Empty, string.Empty, string.Empty);
        var validator = new UpdateCustomerValidator();

        var result = validator.TestValidate(updateCommand);
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void must_fail_with_null_or_empty_lastname()
    {
        // Arrange
        var updateCommand = new UpdateCustomer(0, "john", string.Empty, string.Empty, string.Empty);
        var validator = new UpdateCustomerValidator();

        var result = validator.TestValidate(updateCommand);
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void must_success_with_valid_lastname()
    {
        // Arrange
        var updateCommand = new UpdateCustomer(0, "john", "stone", string.Empty, string.Empty);
        var validator = new UpdateCustomerValidator();

        var result = validator.TestValidate(updateCommand);
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void must_fail_with_empty_id()
    {
        // Arrange
        var updateCommand = new UpdateCustomer(0, "john", string.Empty, string.Empty, string.Empty);
        var validator = new UpdateCustomerValidator();

        var result = validator.TestValidate(updateCommand);
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void must_success_with_valid_id()
    {
        // Arrange
        var updateCommand = new UpdateCustomer(120, "john", string.Empty, string.Empty, string.Empty);
        var validator = new UpdateCustomerValidator();

        var result = validator.TestValidate(updateCommand);
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }
}
