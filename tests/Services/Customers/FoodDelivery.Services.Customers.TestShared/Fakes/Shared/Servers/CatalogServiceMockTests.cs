using System.Net.Http.Json;
using BuildingBlocks.Web.Extensions;
using FoodDelivery.Services.Customers.Shared.Clients.Catalogs;
using FoodDelivery.Services.Customers.Shared.Clients.Catalogs.Dtos;
using FluentAssertions;
using Tests.Shared.Helpers;
using Tests.Shared.XunitCategories;

namespace FoodDelivery.Services.Customers.TestShared.Fakes.Shared.Servers;

public class CatalogServiceMockTests
{
    private readonly CatalogsServiceMock _catalogsServiceMock;

    public CatalogServiceMockTests()
    {
        _catalogsServiceMock = CatalogsServiceMock.Start(ConfigurationHelper.BindOptions<CatalogsApiClientOptions>());
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public async Task root_address()
    {
        var client = new HttpClient { BaseAddress = new Uri(_catalogsServiceMock.Url!) };
        var res = await client.GetAsync("/");
        res.EnsureSuccessStatusCode();

        var g = await res.Content.ReadAsStringAsync();
        g.Should().NotBeEmpty();
        g.Should().Be("Catalogs Service!");
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public async Task get_by_id()
    {
        var (response, endpoint) = _catalogsServiceMock.SetupGetProductById();
        var fakeProduct = response.Product;

        var client = new HttpClient { BaseAddress = new Uri(_catalogsServiceMock.Url!) };
        var httpResponse = await client.GetAsync(endpoint);

        await httpResponse.EnsureSuccessStatusCodeWithDetailAsync();
        var data = await httpResponse.Content.ReadFromJsonAsync<GetProductByIdClientDto>();
        data.Should().NotBeNull();
        data!.Product.Should().BeEquivalentTo(fakeProduct, options => options.ExcludingMissingMembers());
    }
}
