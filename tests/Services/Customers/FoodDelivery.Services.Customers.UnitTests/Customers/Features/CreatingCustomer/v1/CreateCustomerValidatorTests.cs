using FluentAssertions;
using FluentValidation.TestHelper;
using FoodDelivery.Services.Customers.Customers.Features.CreatingCustomer.v1;
using FoodDelivery.Services.Customers.UnitTests.Common;
using Tests.Shared.XunitCategories;

namespace FoodDelivery.Services.Customers.UnitTests.Customers.Features.CreatingCustomer.v1;

public class CreateCustomerValidatorTests : CustomerServiceUnitTestBase
{
    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void must_success_with_valid_inputs()
    {
        // Arrange
        var command = new CreateCustomer("test@example.com");
        var validator = new CreateCustomerValidator();

        var result = validator.TestValidate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void must_success_with_valid_email_input()
    {
        // Arrange
        var command = new CreateCustomer("test@example.com");
        var validator = new CreateCustomerValidator();

        var result = validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void must_fail_with_null_or_empty_email()
    {
        // Arrange
        var command = new CreateCustomer(null!);
        var validator = new CreateCustomerValidator();

        var result = validator.TestValidate(command);
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void must_fail_with_invalid_email()
    {
        // Arrange
        var command = new CreateCustomer("invalid_email");
        var validator = new CreateCustomerValidator();

        var result = validator.TestValidate(command);
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("Email address is invalid.");
    }
}
