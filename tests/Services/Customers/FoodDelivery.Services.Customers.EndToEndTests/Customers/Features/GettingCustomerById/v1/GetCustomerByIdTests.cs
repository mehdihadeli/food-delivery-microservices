using BuildingBlocks.Core.Exception;
using FluentAssertions;
using FoodDelivery.Services.Customers.Api;
using FoodDelivery.Services.Customers.Customers.Dtos.v1;
using FoodDelivery.Services.Customers.Customers.Exceptions.Application;
using FoodDelivery.Services.Customers.Customers.Features.GettingCustomerById.v1;
using FoodDelivery.Services.Customers.Customers.Models.Reads;
using FoodDelivery.Services.Customers.Shared.Data;
using FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Models.Read;
using Microsoft.AspNetCore.Mvc;
using Tests.Shared.Fixtures;
using Tests.Shared.XunitCategories;
using Guid = System.Guid;

namespace FoodDelivery.Services.Customers.EndToEndTests.Customers.Features.GettingCustomerById.v1;

public class GetCustomerByIdTests(
    SharedFixtureWithEfCoreAndMongo<CustomersApiMetadata, CustomersDbContext, CustomersReadDbContext> sharedFixture,
    ITestOutputHelper outputHelper
) : CustomerServiceEndToEndTestBase(sharedFixture, outputHelper)
{
    [Fact]
    [CategoryTrait(TestCategory.EndToEnd)]
    public async Task can_returns_ok_status_code_using_valid_id_and_auth_credentials()
    {
        // Arrange
        var fakeCustomer = new FakeCustomerReadModel().Generate();
        await SharedFixture.InsertMongoDbContextAsync(fakeCustomer);

        var route = Constants.Routes.Customers.GetById(fakeCustomer.Id);

        // Act
        var response = await SharedFixture.NormalUserHttpClient.GetAsync(
            new Uri(SharedFixture.NormalUserHttpClient.BaseAddress!, route),
            TestContext.Current.CancellationToken
        );

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
        var response = await SharedFixture.NormalUserHttpClient.GetAsync(
            new Uri(SharedFixture.NormalUserHttpClient.BaseAddress!, route),
            TestContext.Current.CancellationToken
        );

        // Assert
        response
            .Should()
            .Satisfy<GetCustomerByIdResponse>(x =>
                x.Customer.Should()
                    .BeEquivalentTo(
                        fakeCustomer,
                        o =>
                            o.WithMapping(nameof(CustomerReadModel.FullName), nameof(CustomerReadDto.Name))
                                .ExcludingMissingMembers()
                    )
            );

        // https://github.com/adrianiftode/FluentAssertions.Web
        // // OR
        // response
        //     .Should()
        //     .BeAs(
        //         new
        //         {
        //             CustomerReadModel = new
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
        //              CustomerReadModel = new
        //              {
        //                  Id = default(Guid),
        //                  CustomerId = default(long),
        //                  IdentityId = default(Guid)
        //              }
        //          },
        //          assertion: model =>
        //          {
        //              model.CustomerReadModel.CustomerId.Should().Be(fakeCustomer.CustomerId);
        //              model.CustomerReadModel.Id.Should().Be(fakeCustomer.Id);
        //              model.CustomerReadModel.IdentityId.Should().Be(fakeCustomer.IdentityId);
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
        var response = await SharedFixture.AdminHttpClient.GetAsync(
            new Uri(SharedFixture.NormalUserHttpClient.BaseAddress!, route),
            TestContext.Current.CancellationToken
        );

        // Assert
        response
            .Should()
            .Satisfy<ProblemDetails>(pr =>
            {
                pr.Detail.Should().Be($"CustomerReadModel with id '{notExistsId}' not found.");
                pr.Title.Should().Be(nameof(CustomerNotFoundException));
                pr.Type.Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.5");
            })
            .And.Be404NotFound();

        // // OR
        // response
        //     .Should()
        //     .HaveError("title", nameof(CustomerNotFoundException).Humanize(LetterCasing.Title))
        //     .And.HaveError("type", "https://tools.ietf.org/html/rfc9110#section-15.5.5")
        //     .And.HaveErrorMessage($"CustomerReadModel with id '{notExistsId}' not found.")
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
        var response = await SharedFixture.AdminHttpClient.GetAsync(
            new Uri(SharedFixture.NormalUserHttpClient.BaseAddress!, route),
            TestContext.Current.CancellationToken
        );

        // Assert
        response
            .Should()
            .Satisfy<ProblemDetails>(pr =>
            {
                pr.Detail.Should().Contain("'Id' must not be empty.");
                pr.Title.Should().Be(nameof(ValidationException));
                pr.Type.Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.1");
            })
            .And.Be400BadRequest();

        // // OR
        // response
        //     .Should()
        //     .ContainsProblemDetail(
        //         new ProblemDetails
        //         {
        //             Detail = "'Id' must not be empty.",
        //             Title = nameof(ValidationException).Humanize(LetterCasing.Title),
        //             Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
        //         }
        //     )
        //     .And.Be400BadRequest();
    }
}
