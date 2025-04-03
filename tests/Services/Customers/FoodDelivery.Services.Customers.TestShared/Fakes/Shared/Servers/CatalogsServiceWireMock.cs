using System.Net;
using FoodDelivery.Services.Customers.Shared.Clients.Rest.Catalogs.Dtos;
using FoodDelivery.Services.Customers.Shared.Clients.Rest.Catalogs.Rest;
using FoodDelivery.Services.Customers.TestShared.Fakes.Shared.Dtos;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace FoodDelivery.Services.Customers.TestShared.Fakes.Shared.Servers;

//https://www.ontestautomation.com/api-mocking-in-csharp-with-wiremock-net/
public class CatalogsServiceWireMock(WireMockServer wireMockServer, CatalogsRestClientOptions catalogsRestClientOption)
{
    private CatalogsRestClientOptions CatalogsRestClientOptions { get; } = catalogsRestClientOption;

    public (GetProductByIdClientDto Response, string Endpoint) SetupGetProductById(long id = 0)
    {
        var fakeProduct = new FakeProductDto().Generate(1).First();
        if (id > 0)
            fakeProduct = fakeProduct with { Id = id };

        fakeProduct = fakeProduct with { AvailableStock = 0 };

        var response = new GetProductByIdClientDto(fakeProduct);

        //https://github.com/WireMock-Net/WireMock.Net/wiki/Request-Matching
        // we should put / in the beginning of the endpoint
        var endpointPath = $"/{CatalogsRestClientOptions.GetProductByIdEndpoint}/{fakeProduct.Id}";

        wireMockServer
            .Given(Request.Create().UsingGet().WithPath(endpointPath))
            .RespondWith(Response.Create().WithBodyAsJson(response).WithStatusCode(HttpStatusCode.OK));

        return (response, endpointPath);
    }
}
