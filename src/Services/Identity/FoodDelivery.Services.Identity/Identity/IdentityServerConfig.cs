using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using IdentityModel;

namespace FoodDelivery.Services.Identity.Identity;

// Ref: https://docs.duendesoftware.com/identityserver/v5/fundamentals/resources/api_resources/
// https://docs.duendesoftware.com/identityserver/v5/fundamentals/resources/identity/
// https://docs.duendesoftware.com/identityserver/v5/fundamentals/resources/api_scopes/
public static class IdentityServerConfig
{
    // An identity resource is a named group of claims about a user that can be requested using the scope parameter.
    public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResources.Phone(),
            new IdentityResources.Address(),
            new("roles", "User Roles", new List<string> { "role" }),
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope> { new("food-delivery-api", "FoodDelivery.Services.Catalogs Web API") };

    public static IList<ApiResource> ApiResources =>
        new List<ApiResource>
        {
            new ApiResource("ShopApiResource", "FoodDelivery.Services.Catalogs Web API Resource")
            {
                Scopes = { "food-delivery-api" },
                UserClaims = { JwtClaimTypes.Role, JwtClaimTypes.Name, JwtClaimTypes.Id },
            },
        };

    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            new()
            {
                ClientId = "frontend-client",
                ClientName = "Frontend Client",
                RequireClientSecret = false,
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    "roles",
                    "food-delivery-api",
                },
            },
            new()
            {
                ClientId = "oauthClient",
                ClientName = "Example client application using client credentials",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = new List<Secret> { new("SuperSecretPassword".Sha256()) }, // change me!
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    "roles",
                    "food-delivery-api",
                },
            },
        };
}
