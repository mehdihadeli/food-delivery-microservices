using BuildingBlocks.Core.Exception.Types;
using BuildingBlocks.Validation;
using FluentAssertions;
using FoodDelivery.Services.Customers.Api;
using FoodDelivery.Services.Customers.Customers.Exceptions.Application;
using FoodDelivery.Services.Customers.Customers.Features.GettingCustomerById.v1;
using FoodDelivery.Services.Customers.Shared.Data;
using FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Entities;
using Microsoft.AspNetCore.Mvc;
using Tests.Shared.Extensions;
using Tests.Shared.Fixtures;
using Tests.Shared.XunitCategories;
using Xunit.Abstractions;
using Guid = System.Guid;

namespace FoodDelivery.Services.Customers.EndToEndTests.Customers.Features.GettingCustomerById.v1;

public class GetCustomerByIdTests : CustomerServiceEndToEndTestBase
{
    public GetCustomerByIdTests(
        SharedFixtureWithEfCoreAndMongo<CustomersApiMetadata, CustomersDbContext, CustomersReadDbContext> sharedFixture,
        ITestOutputHelper outputHelper
    )
        : base(sharedFixture, outputHelper) { }

    [Fact]
    [CategoryTrait(TestCategory.EndToEnd)]
    public async Task can_returns_ok_status_code_using_valid_id_and_auth_credentials()
    {
        // Arrange
        var fakeCustomer = new FakeCustomerReadModel().Generate();
        await SharedFixture.InsertMongoDbContextAsync(fakeCustomer);

        var route = Constants.Routes.Customers.GetById(fakeCustomer.Id);

        // Act
        var response = await SharedFixture.NormalUserHttpClient.GetAsync(route);

        // Assert
        response.Should().Be200Ok();
    }

    [Fact]
    [CategoryTrait(TestCategory.EndToEnd)]
    public async Task can_returns_valid_response_using_valid_id_and_auth_credentials()
    {
        // Arrange
        var fakeCustomer = new FakeCustomerReadModel().Generate();
        await SharedFixture.InsertMongoDbContextAsync(fakeCustomer);

        var route = Constants.Routes.Customers.GetById(fakeCustomer.Id);

        // Act
        var response = await SharedFixture.NormalUserHttpClient.GetAsync(route);

        // Assert
        response.Should().Satisfy<GetCustomerByIdResponse>(x => x.Customer.Should().BeEquivalentTo(fakeCustomer));

        // // OR
        // response
        //     .Should()
        //     .BeAs(
        //         new
        //         {
        //             Customer = new
        //             {
        //                 Id = fakeCustomer.Id,
        //                 CustomerId = fakeCustomer.CustomerId,
        //                 IdentityId = fakeCustomer.IdentityId
        //             }
        //         }
        //     );

        // // OR
        //  response
        //      .Should()
        //      .Satisfy(
        //          givenModelStructure: new
        //          {
        //              Customer = new
        //              {
        //                  Id = default(Guid),
        //                  CustomerId = default(long),
        //                  IdentityId = default(Guid)
        //              }
        //          },
        //          assertion: model =>
        //          {
        //              model.Customer.CustomerId.Should().Be(fakeCustomer.CustomerId);
        //              model.Customer.Id.Should().Be(fakeCustomer.Id);
        //              model.Customer.IdentityId.Should().Be(fakeCustomer.IdentityId);
        //          }
        //      );
    }

    [Fact]
    [CategoryTrait(TestCategory.EndToEnd)]
    public async Task must_returns_not_found_status_code_when_customer_not_exists()
    {
        // Arrange
        var notExistsId = Guid.NewGuid();
        var route = Constants.Routes.Customers.GetById(notExistsId);

        // Act
        var response = await SharedFixture.AdminHttpClient.GetAsync(route);

        // Assert
        response
            .Should()
            .Satisfy<ProblemDetails>(pr =>
            {
                pr.Detail.Should().Be($"Customer with id '{notExistsId}' not found.");
                pr.Title.Should().Be(nameof(CustomerNotFoundException));
                pr.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.4");
            })
            .And.Be404NotFound();

        // // OR
        // response
        //     .Should()
        //     .HaveError("title", nameof(CustomerNotFoundException))
        //     .And.HaveError("type", "https://tools.ietf.org/html/rfc7231#section-6.5.4")
        //     .And.HaveErrorMessage($"Customer with id '{notExistsId}' not found.")
        //     .And.Be404NotFound();
    }

    [Fact]
    [CategoryTrait(TestCategory.EndToEnd)]
    public async Task must_returns_bad_request_status_code_with_invalid()
    {
        // Arrange
        var invalidId = Guid.Empty;
        var route = Constants.Routes.Customers.GetById(invalidId);

        // Act
        var response = await SharedFixture.AdminHttpClient.GetAsync(route);

        // Assert

        response
            .Should()
            .Satisfy<ProblemDetails>(pr =>
            {
                pr.Detail.Should().Be("'Id' must not be empty.");
                pr.Title.Should().Be(nameof(ValidationException));
                pr.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.1");
            })
            .And.Be400BadRequest();

        // // OR
        // response
        //     .Should()
        //     .ContainsProblemDetail(
        //         new ProblemDetails
        //         {
        //             Detail = "'Id' must not be empty.",
        //             Title = nameof(ValidationException),
        //             Type = "https://somedomain/input-validation-rules-error",
        //         }
        //     )
        //     .And.Be400BadRequest();
    }
}
