using Bogus;
using ECommerce.Services.Identity.Users.Features.GettingUserById.v1;
using ECommerce.Services.Identity.Users.Features.RegisteringUser.v1;
using ECommerce.Services.Shared.Identity.Users.Events.v1.Integration;
using Microsoft.Extensions.DependencyInjection;
using Tests.Shared.Fixtures;
using Xunit.Abstractions;

namespace ECommerce.Services.Identity.IntegrationTests.Users.Features.RegisteringUser.v1;

public class RegisterUserTests : IntegrationTestBase<Program>
{
     private static RegisterUser _registerUser;

    public RegisterUserTests(IntegrationTestFixture<Program> integrationTestFixture,
        ITestOutputHelper outputHelper) : base(integrationTestFixture, outputHelper)
    {
        // Arrange
        _registerUser = new Faker<RegisterUser>().CustomInstantiator(faker =>
                new RegisterUser(
                    faker.Person.FirstName,
                    faker.Person.LastName,
                    faker.Person.UserName,
                    faker.Person.Email,
                    "123456",
                    "123456"))
            .Generate();
    }

    protected override void RegisterTestsServices(IServiceCollection services)
    {
        base.RegisterTestsServices(services);
        // services.ReplaceScoped<IDataSeeder, CustomersTestDataSeeder>();
    }

    [Fact]
    public async Task register_new_user_command_should_persist_new_user_in_db()
    {
        // Act
        var result = await IntegrationTestFixture.SendAsync(_registerUser, CancellationToken);

        // Assert
        result.UserIdentity.Should().NotBeNull();

        // var user = await IdentityModule.FindWriteAsync<ApplicationUser>(result.UserIdentity.Id);
        // user.Should().NotBeNull();

        var userByIdResponse =
            await IntegrationTestFixture.QueryAsync(new GetUserById(result.UserIdentity.Id));

        userByIdResponse.IdentityUser.Should().NotBeNull();
        userByIdResponse.IdentityUser.Id.Should().Be(result.UserIdentity.Id);
    }

    [Fact]
    public async Task register_new_user_command_should_publish_message_to_broker()
    {
        // Act
        await IntegrationTestFixture.SendAsync(_registerUser, CancellationToken);

        // Assert
        var res = await IntegrationTestFixture.IsPublished<UserRegistered>();
        res.Should().BeTrue();
    }
}
