using Bogus;
using FluentAssertions;
using FoodDelivery.Services.Identity.Api;
using FoodDelivery.Services.Identity.Users.Features.GettingUserById.v1;
using FoodDelivery.Services.Identity.Users.Features.RegisteringUser.v1;
using FoodDelivery.Services.Shared;
using FoodDelivery.Services.Shared.Identity.Users.Events.Integration.v1;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Tests.Shared.Fixtures;
using Xunit;

namespace FoodDelivery.Services.Identity.IntegrationTests.Users.Features.RegisteringUser.v1;

public class RegisterUserTests : IdentityServiceIntegrationTestBase
{
    private static RegisterUser _registerUser = default!;

    public RegisterUserTests(
        SharedFixtureWithEfCore<IdentityApiMetadata, IdentityDbContext> sharedFixture,
        ITestOutputHelper outputHelper
    )
        : base(sharedFixture, outputHelper)
    {
        // Arrange
        _registerUser = new Faker<RegisterUser>()
            .CustomInstantiator(faker => new RegisterUser(
                faker.Person.FirstName,
                faker.Person.LastName,
                faker.Person.UserName,
                faker.Person.Email,
                faker.Phone.PhoneNumber("(+##)##########"),
                "123456",
                "123456",
                [Authorization.Roles.User],
                null
            ))
            .Generate();
    }

    [Fact]
    public async Task register_new_user_command_should_persist_new_user_in_db()
    {
        // Act
        var result = await SharedFixture.CommandAsync(_registerUser, CancellationToken);

        // Assert
        result.UserIdentity.Should().NotBeNull();

        // var user = await IdentityModule.FindWriteAsync<ApplicationUser>(result.UserIdentity.InternalCommandId);
        // user.Should().NotBeNull();

        var userByIdResponse = await SharedFixture.QueryAsync(new GetUserById(result.UserIdentity!.Id));

        userByIdResponse.IdentityUser.Should().NotBeNull();
        userByIdResponse.IdentityUser.Id.Should().Be(result.UserIdentity.Id);
    }

    [Fact]
    public async Task register_new_user_command_should_publish_message_to_broker()
    {
        // Act
        await SharedFixture.CommandAsync(_registerUser, CancellationToken);

        // Assert
        await SharedFixture.ShouldPublishing<UserRegisteredV1>();
    }
}
