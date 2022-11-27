using ECommerce.Services.Customers.Shared.Clients.Catalogs;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;

namespace ECommerce.Services.Customers.IntegrationTests.Mocks.Servers;

//https://www.ontestautomation.com/api-mocking-in-csharp-with-wiremock-net/
public class CatalogsServiceMock : WireMockServer
{
    private CatalogsApiClientOptions CatalogsApiClientOptions { get; init; } = default!;

    protected CatalogsServiceMock(WireMockServerSettings settings) : base(settings)
    {
        //https://github.com/WireMock-Net/WireMock.Net/wiki/Request-Matching
        Given(Request.Create().WithPath("/").UsingGet()) // we should put / in the beginning of the endpoint
            .RespondWith(
                Response.Create()
                    .WithStatusCode(200)
                    .WithBody("Catalogs Service!")
            );
    }

    public static CatalogsServiceMock Start(CatalogsApiClientOptions catalogsApiClientOptions, bool ssl = false)
    {
        // new WireMockServer() is equivalent to call WireMockServer.Start()
        return new CatalogsServiceMock(new WireMockServerSettings
        {
            UseSSL = ssl,
            //// we could use our option url here, but I use random port
            Urls = new []{catalogsApiClientOptions.BaseApiAddress}
        })
        {
            CatalogsApiClientOptions = catalogsApiClientOptions
        };
    }
}
