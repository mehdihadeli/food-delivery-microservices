using System.Net.Http.Json;
using ECommerce.Services.Customers.Shared.Clients.Identity;
using ECommerce.Services.Customers.Shared.Clients.Identity.Dtos;
using FluentAssertions;
using Tests.Shared.Helpers;

namespace ECommerce.Services.Customers.IntegrationTests.Mocks.Servers;

public class IdentityServiceMockTests
{
    private readonly IdentityServiceMock _identityServiceMock;
    private readonly IdentityApiClientOptions _identityApiClientOptions;


    public IdentityServiceMockTests()
    {
        _identityServiceMock = IdentityServiceMock.Start(ConfigurationHelper.BindOptions<IdentityApiClientOptions>());
        _identityApiClientOptions = ConfigurationHelper.BindOptions<IdentityApiClientOptions>();
    }

    [Fact]
    public async Task Root_Address()
    {
        var client = new HttpClient {BaseAddress = new Uri(_identityServiceMock.Url)};
        var res = await client.GetAsync("/");
        res.EnsureSuccessStatusCode();

        var g = await res.Content.ReadAsStringAsync();
        g.Should().NotBeEmpty();
    }


    [Fact]
    public async Task Get_By_Email()
    {
        var email = "test@example.com";
        _identityServiceMock.SetupGetUserByEmail(email,
            new GetUserByEmailResponse(new UserIdentityDto {Email = email}));

        var endpointPath = $"{_identityApiClientOptions.UsersEndpoint}/by-email/{email}";

        var client = new HttpClient {BaseAddress = new Uri(_identityServiceMock.Url)};
        var res = await client.GetAsync(endpointPath);
        res.EnsureSuccessStatusCode();

        var g = await res.Content.ReadFromJsonAsync<GetUserByEmailResponse>();
        g.Should().NotBeNull();
    }
}
