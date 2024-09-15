using BuildingBlocks.Core.Exception.Types;
using FluentAssertions;
using FoodDelivery.Services.Customers.Api;
using FoodDelivery.Services.Customers.Customers.Exceptions.Application;
using FoodDelivery.Services.Customers.Customers.Features.CreatingCustomer.v1;
using FoodDelivery.Services.Customers.Shared.Data;
using FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Entities;
using FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Requests;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Tests.Shared.Extensions;
using Tests.Shared.Fixtures;
using Tests.Shared.XunitCategories;
using Xunit.Abstractions;

namespace FoodDelivery.Services.Customers.EndToEndTests.Customers.Features.CreatingCustomer.v1;

public class CreateCustomerTests : CustomerServiceEndToEndTestBase
{
    public CreateCustomerTests(
        SharedFixtureWithEfCoreAndMongo<CustomersApiMetadata, CustomersDbContext, CustomersReadDbContext> sharedFixture,
        ITestOutputHelper outputHelper
    )
        : base(sharedFixture, outputHelper)
    {
        AssertionOptions.AssertEquivalencyUsing(options => options.ExcludingMissingMembers());
    }

    [Fact]
    [CategoryTrait(TestCategory.EndToEnd)]
    public async Task can_returns_created_status_code_using_valid_dto_and_auth_credentials()
    {
        // Arrange
        var fakeIdentityUser = IdentityServiceWireMock.SetupGetUserByEmail().Response.UserIdentity;
        var fakeCreateCustomerRequest = new FakeCreateCustomerRequest(fakeIdentityUser!.Email).Generate();
        var route = Constants.Routes.Customers.Create;

        // Act
        var response = await SharedFixture.AdminHttpClient.PostAsJsonAsync(route, fakeCreateCustomerRequest);

        // Assert
        response.Should().Be201Created();
    }

    [Fact]
    [CategoryTrait(TestCategory.EndToEnd)]
    public async Task can_returns_valid_response_using_valid_dto_and_auth_credentials()
    {
        // Arrange
        var fakeIdentityUser = IdentityServiceWireMock.SetupGetUserByEmail().Response.UserIdentity;
        var fakeCreateCustomerRequest = new FakeCreateCustomerRequest(fakeIdentityUser!.Email).Generate();
        var route = Constants.Routes.Customers.Create;

        // Act
        var response = await SharedFixture.AdminHttpClient.PostAsJsonAsync(route, fakeCreateCustomerRequest);

        //response.Should().Satisfy<CreateCustomerResponse>(x => x.CustomerId.Should().BeGreaterThan(0));

        // Assert
        response
            .Should()
            .HasResponse<CreateCustomerResponse>(responseAction: customerResponse =>
            {
                customerResponse!.CustomerId.Should().BeGreaterThan(0);
            });
    }

    [Fact]
    [CategoryTrait(TestCategory.EndToEnd)]
    public async Task must_returns_conflict_status_code_when_customer_already_exists()
    {
        // Arrange
        var fakeCustomer = new FakeCustomer().Generate();
        await SharedFixture.InsertEfDbContextAsync(fakeCustomer);
        var createCustomerRequest = new CreateCustomerRequest(fakeCustomer.Email);
        var route = Constants.Routes.Customers.Create;

        // Act
        var response = await SharedFixture.AdminHttpClient.PostAsJsonAsync(route, createCustomerRequest);

        // Assert
        response
            .Should()
            .HasProblemDetail(
                new
                {
                    Detail = $"Customer with email '{fakeCustomer.Email.Value}' already exists.",
                    Title = nameof(CustomerAlreadyExistsException).Humanize(LetterCasing.Title),
                }
            )
            .And.Be409Conflict();
    }

    [Fact]
    [CategoryTrait(TestCategory.EndToEnd)]
    public async Task must_returns_bad_request_status_code_when_email_is_invalid()
    {
        // Arrange
        var invalidEmail = "invalid_email";
        var createCustomerRequest = new CreateCustomerRequest(invalidEmail);
        var route = Constants.Routes.Customers.Create;

        // Act
        var response = await SharedFixture.AdminHttpClient.PostAsJsonAsync(route, createCustomerRequest);

        // Assert
        response
            .Should()
            .ContainsProblemDetail(
                new ProblemDetails
                {
                    Detail = "Email address is invalid.",
                    Title = nameof(ValidationException).Humanize(LetterCasing.Title),
                }
            )
            .And.Be400BadRequest();
    }
}
