using System.Net;
using FoodDelivery.Services.Customers.Products.Models;
using FoodDelivery.Services.Customers.Shared.Clients.Catalogs;
using FoodDelivery.Services.Customers.Shared.Clients.Catalogs.Dtos;
using FoodDelivery.Services.Customers.TestShared.Fakes.Shared.Dtos;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;

namespace FoodDelivery.Services.Customers.TestShared.Fakes.Shared.Servers;

//https://www.ontestautomation.com/api-mocking-in-csharp-with-wiremock-net/
public class CatalogsServiceMock : WireMockServer
{
    private CatalogsApiClientOptions CatalogsApiClientOptions { get; init; } = default!;

    protected CatalogsServiceMock(WireMockServerSettings settings)
        : base(settings)
    {
        //https://github.com/WireMock-Net/WireMock.Net/wiki/Request-Matching
        Given(Request.Create().WithPath("/").UsingGet()) // we should put / in the beginning of the endpoint
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("Catalogs Service!"));
    }

    public static CatalogsServiceMock Start(CatalogsApiClientOptions catalogsApiClientOptions, bool ssl = false)
    {
        // new WireMockServer() is equivalent to call WireMockServer.Start()
        return new CatalogsServiceMock(
            new WireMockServerSettings
            {
                UseSSL = ssl,
                // we could use our option url here, but I use random port (Urls = new string[] {} also set a fix port 5000 we should not use this if we want a random port)
                // Urls = new string[]{catalogsApiClientOptions.BaseApiAddress}
            }
        )
        {
            CatalogsApiClientOptions = catalogsApiClientOptions
        };
    }

    public (GetProductByIdClientDto Response, string Endpoint) SetupGetProductById(long id = 0)
    {
        var fakeProduct = new FakeProductDto().Generate(1).First();
        if (id > 0)
            fakeProduct = fakeProduct with { Id = id };

        fakeProduct = fakeProduct with { AvailableStock = 0 };

        var response = new GetProductByIdClientDto(fakeProduct);

        //https://github.com/WireMock-Net/WireMock.Net/wiki/Request-Matching
        // we should put / in the beginning of the endpoint
        var endpointPath = $"/{CatalogsApiClientOptions.ProductsEndpoint}/{fakeProduct.Id}";

        Given(Request.Create().UsingGet().WithPath(endpointPath))
            .RespondWith(Response.Create().WithBodyAsJson(response).WithStatusCode(HttpStatusCode.OK));

        return (response, endpointPath);
    }
}
