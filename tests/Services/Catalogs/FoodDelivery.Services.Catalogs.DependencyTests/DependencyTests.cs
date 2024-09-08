using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Core.Web;
using FoodDelivery.Services.Catalogs.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FoodDelivery.Services.Catalogs.DependencyTests;

public class DependencyTests
{
    [Fact]
    public void validate_service_dependencies()
    {
        var factory = new WebApplicationFactory<CatalogsApiMetadata>().WithWebHostBuilder(webHostBuilder =>
        {
            webHostBuilder.UseEnvironment(Environments.DependencyTest);

            webHostBuilder.ConfigureTestServices(services =>
            {
                services.TryAddTransient<IServiceCollection>(_ => services);
            });
        });

        using var scope = factory.Services.CreateScope();
        var sp = scope.ServiceProvider;
        var services = sp.GetRequiredService<IServiceCollection>();
        sp.ValidateDependencies(services, typeof(CatalogsApiMetadata).Assembly, typeof(CatalogsMetadata).Assembly);
    }
}
