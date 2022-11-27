using System.Net;
using ECommerce.Services.Customers.Shared.Clients.Identity;
using ECommerce.Services.Customers.Shared.Clients.Identity.Dtos;
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
            //// we could use our option url here, but I use random port
            Urls = new []{identityApiClientOptions.BaseApiAddress}
        }) {IdentityApiClientOptions = identityApiClientOptions};

        return mock;
    }

    public IdentityServiceMock SetupGetUserByEmail(string email, GetUserByEmailResponse response)
    {
        //https://github.com/WireMock-Net/WireMock.Net/wiki/Request-Matching
        var endpointPath = $"/{IdentityApiClientOptions.UsersEndpoint}/by-email/{email}"; // we should put / in the beginning of the endpoint

        Given(Request.Create().UsingGet().WithPath(endpointPath))
            .RespondWith(Response.Create()
                .WithBodyAsJson(response)
                .WithStatusCode(HttpStatusCode.OK));

        return this;
    }
}
