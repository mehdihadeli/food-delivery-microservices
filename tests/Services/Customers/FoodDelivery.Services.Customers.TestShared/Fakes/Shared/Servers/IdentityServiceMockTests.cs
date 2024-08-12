using System.Net.Http.Json;
using BuildingBlocks.Web.Extensions;
using FluentAssertions;
using FoodDelivery.Services.Customers.Shared.Clients.Identity;
using FoodDelivery.Services.Customers.Shared.Clients.Identity.Dtos;
using FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Events;
using Tests.Shared.Helpers;
using Tests.Shared.XunitCategories;

namespace FoodDelivery.Services.Customers.TestShared.Fakes.Shared.Servers;

public class IdentityServiceMockTests
{
    private readonly IdentityServiceMock _identityServiceMock;

    public IdentityServiceMockTests()
    {
        _identityServiceMock = IdentityServiceMock.Start(ConfigurationHelper.BindOptions<IdentityApiClientOptions>());
    }

    [Fact]
    public async Task root_address()
    {
        var client = new HttpClient { BaseAddress = new Uri(_identityServiceMock.Url!) };
        var res = await client.GetAsync("/");
        res.EnsureSuccessStatusCode();

        var g = await res.Content.ReadAsStringAsync();
        g.Should().NotBeEmpty();
        g.Should().Be("Identity Service!");
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public async Task get_by_email()
    {
        var (response, endpoint) = _identityServiceMock.SetupGetUserByEmail();
        var fakeIdentityUser = response.UserIdentity;

        var client = new HttpClient { BaseAddress = new Uri(_identityServiceMock.Url!) };
        var httpResponse = await client.GetAsync(endpoint);

        await httpResponse.EnsureSuccessStatusCodeWithDetailAsync();
        var data = await httpResponse.Content.ReadFromJsonAsync<GetUserByEmailClientDto>();
        data.Should().NotBeNull();
        data!.UserIdentity.Should().BeEquivalentTo(fakeIdentityUser, options => options.ExcludingMissingMembers());
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public async Task get_by_email_and_user_registered()
    {
        var userRegistered = new FakeUserRegisteredV1().Generate();
        var (response, endpoint) = _identityServiceMock.SetupGetUserByEmail(userRegistered);
        var fakeIdentityUser = response.UserIdentity;

        var client = new HttpClient { BaseAddress = new Uri(_identityServiceMock.Url!) };
        var res = await client.GetAsync(endpoint);
        res.EnsureSuccessStatusCode();

        var g = await res.Content.ReadFromJsonAsync<GetUserByEmailClientDto>();
        g.Should().NotBeNull();
        g!.UserIdentity.Should().BeEquivalentTo(fakeIdentityUser, options => options.ExcludingMissingMembers());
    }
}
