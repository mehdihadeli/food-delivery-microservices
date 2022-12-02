using Ardalis.GuardClauses;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using ECommerce.Services.Customers.Shared.Clients.Catalogs;
using ECommerce.Services.Customers.Shared.Clients.Identity;
using Microsoft.Extensions.Options;

namespace ECommerce.Services.Customers.Shared.Extensions.WebApplicationBuilderExtensions;

public static partial class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddCustomHttpClients(
        this WebApplicationBuilder builder)
    {
        builder.Services.AddValidatedOptions<IdentityApiClientOptions>();
        builder.Services.AddValidatedOptions<CatalogsApiClientOptions>();

        builder.Services.AddHttpClient<ICatalogApiClient, CatalogApiClient>((client, sp) =>
        {
            var catalogApiOptions = sp.GetRequiredService<IOptions<CatalogsApiClientOptions>>();
            Guard.Against.Null(catalogApiOptions.Value);

            var baseAddress = catalogApiOptions.Value.BaseApiAddress;
            client.BaseAddress = new Uri(baseAddress);
            return new CatalogApiClient(client, catalogApiOptions);
        });

        builder.Services.AddHttpClient<IIdentityApiClient, IdentityApiClient>((client, sp) =>
        {
            var identityApiOptions = sp.GetRequiredService<IOptions<IdentityApiClientOptions>>();
            Guard.Against.Null(identityApiOptions.Value);

            var baseAddress = identityApiOptions.Value.BaseApiAddress;
            client.BaseAddress = new Uri(baseAddress);
            return new IdentityApiClient(client, identityApiOptions);
        });

        return builder;
    }
}
