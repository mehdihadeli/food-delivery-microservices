using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Resiliency.Extensions;
using ECommerce.Services.Customers.Shared.Clients.Catalogs;
using ECommerce.Services.Customers.Shared.Clients.Identity;
using Microsoft.Extensions.Options;

namespace ECommerce.Services.Customers.Shared.Extensions.WebApplicationBuilderExtensions;

public static partial class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddCustomHttpClients(
        this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<IdentityApiClientOptions>()
            .Bind(builder.Configuration.GetSection(nameof(IdentityApiClientOptions))).ValidateDataAnnotations();

        builder.Services.AddOptions<CatalogsApiClientOptions>()
            .Bind(builder.Configuration.GetSection(nameof(CatalogsApiClientOptions)))
            .ValidateDataAnnotations();

        builder.Services.AddHttpClient<ICatalogApiClient, CatalogApiClient>((client, sp) =>
        {
            var baseAddress = sp.GetRequiredService<IConfiguration>().GetOptions<CatalogsApiClientOptions>()
                .BaseApiAddress;
            client.BaseAddress = new Uri(baseAddress);
            return new CatalogApiClient(client, sp.GetRequiredService<IOptions<CatalogsApiClientOptions>>());
        });

        builder.Services.AddHttpClient<IIdentityApiClient, IdentityApiClient>((client, sp) =>
        {
            var baseAddress = sp.GetRequiredService<IConfiguration>().GetOptions<IdentityApiClientOptions>()
                .BaseApiAddress;
            client.BaseAddress = new Uri(baseAddress);
            return new IdentityApiClient(client, sp.GetRequiredService<IOptions<IdentityApiClientOptions>>());
        });

        return builder;
    }
}
