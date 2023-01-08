using System.Net;
using ECommerce.Services.Customers.Shared.Clients.Identity;
using ECommerce.Services.Customers.Shared.Clients.Identity.Dtos;
using ECommerce.Services.Customers.TestShared.Fakes.Shared.Dtos;
using ECommerce.Services.Shared.Identity.Users.Events.v1.Integration;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;

namespace ECommerce.Services.Customers.IntegrationTests.Mocks.Servers;

//https://www.ontestautomation.com/api-mocking-in-csharp-with-wiremock-net/
//https://github.com/WireMock-Net/WireMock.Net/wiki
//https://pcholko.com/posts/2021-04-05/wiremock-integration-test/
//https://www.youtube.com/watch?v=YU3ohofu6UU
public class IdentityServiceMock : WireMockServer
{
    private IdentityApiClientOptions IdentityApiClientOptions { get; init; } = default!;

    private IdentityServiceMock(WireMockServerSettings settings) : base(settings)
    {
        //https://github.com/WireMock-Net/WireMock.Net/wiki/Request-Matching
        Given(Request.Create().WithPath("/").UsingGet()) // we should put / in the beginning of the endpoint
            .RespondWith(
                Response.Create()
                    .WithStatusCode(200)
                    .WithBody("Identity Service!")
            );
    }

    public static IdentityServiceMock Start(IdentityApiClientOptions identityApiClientOptions, bool ssl = false)
    {
        // new WireMockServer() is equivalent to call WireMockServer.Start()
        var mock = new IdentityServiceMock(new WireMockServerSettings
        {
            UseSSL = ssl,
            // we could use our option url here, but I use random port (Urls = new string[] {} also set a fix port 5000 we should not use this if we want a random port)
            // Urls = new string[] {identityApiClientOptions.BaseApiAddress}
        }) {IdentityApiClientOptions = identityApiClientOptions};

        return mock;
    }

    public (GetUserByEmailResponse Response, string Endpoint) SetupGetUserByEmail(string? email = null)
    {
        var fakeIdentityUser = new FakeUserIdentityDto().Generate(1).First();
        if (!string.IsNullOrWhiteSpace(email))
            fakeIdentityUser = fakeIdentityUser with {Email = email};

        var response = new GetUserByEmailResponse(fakeIdentityUser);

        //https://github.com/WireMock-Net/WireMock.Net/wiki/Request-Matching
        // we should put / in the beginning of the endpoint
        var endpointPath =
            $"/{IdentityApiClientOptions.UsersEndpoint}/by-email/{fakeIdentityUser.Email}";

        Given(Request.Create().UsingGet().WithPath(endpointPath))
            .RespondWith(Response.Create()
                .WithBodyAsJson(response)
                .WithStatusCode(HttpStatusCode.OK));

        return (response, endpointPath);
    }

    public (GetUserByEmailResponse Response, string Endpoint) SetupGetUserByEmail(UserRegisteredV1 userRegisteredV1)
    {
        var response = new GetUserByEmailResponse(
            new UserIdentityDto(
                userRegisteredV1.IdentityId,
                userRegisteredV1.UserName,
                userRegisteredV1.Email,
                userRegisteredV1.PhoneNumber,
                userRegisteredV1.FirstName,
                userRegisteredV1.LastName));

        //https://github.com/WireMock-Net/WireMock.Net/wiki/Request-Matching
        var endpointPath =
            $"/{IdentityApiClientOptions.UsersEndpoint}/by-email/{userRegisteredV1.Email}"; // we should put / in the beginning of the endpoint

        Given(Request.Create().UsingGet().WithPath(endpointPath))
            .RespondWith(Response.Create()
                .WithBodyAsJson(response)
                .WithStatusCode(HttpStatusCode.OK));

        return (response, endpointPath);
    }
}
