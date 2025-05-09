using BuildingBlocks.Core.Security;
using BuildingBlocks.Security.Jwt;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using FoodDelivery.Services.Shared;
using IdentityModel;

namespace FoodDelivery.Services.Identity.Identity;

// https://docs.duendesoftware.com/identityserver/quickstarts/0-overview/
// dotnet new install Duende.Templates - dotnet new  duende-is-aspid
// Ref: https://docs.duendesoftware.com/identityserver/v5/fundamentals/resources/api_resources/
// https://docs.duendesoftware.com/identityserver/v5/fundamentals/resources/identity/
// https://docs.duendesoftware.com/identityserver/v5/fundamentals/resources/api_scopes/
public static class IdentityServerConfig
{
    // Identity Resources (for ID token)
    public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResources.Phone(),
            new IdentityResources.Address(),
        };

    // this apis will show in `aud` section of token
    // - the ApiResource class allows for some additional organization and grouping and isolation of scopes and providing some common settings.
    // - Using the API resource grouping gives you the following additional features:
    //      1. support for the JWT aud claim. The value(s) of the audience claim will be the name of the API resource(s)
    //      2.support for adding common user claims across all contained scopes
    // https://docs.duendesoftware.com/identityserver/fundamentals/resources/api-resources/
    public static IEnumerable<ApiResource> ApiResources =>
        // - The `combination` of allowed scopes (what the client is permitted to do) and user permissions (what the user is permitted to do) ensures security.
        new List<ApiResource>
        {
            new("catalogs-api", "Catalogs API")
            {
                // what client can do
                Scopes = { Scopes.CatalogsRead, Scopes.CatalogsWrite, Scopes.CatalogsFull },
                // shared claims between all scopes
                UserClaims = { JwtClaimTypes.Role, ClaimsType.Permission },
            },
            new("customers-api", "Customers API")
            {
                // what client can do
                Scopes = { Scopes.CustomersRead, Scopes.CustomersWrite, Scopes.CustomersFull },
                // shared claims between all scopes
                UserClaims = { JwtClaimTypes.Role, ClaimsType.Permission },
            },
            new("users-api", "Users API")
            {
                // what client can do
                Scopes = { Scopes.UsersRead, Scopes.UsersWrite, Scopes.UsersFull },
                // shared claims between all scopes
                UserClaims = { JwtClaimTypes.Role, ClaimsType.Permission },
            },
            new("orders-api", "Orders API")
            {
                // what client can do
                Scopes = { Scopes.OrdersRead, Scopes.OrdersWrite, Scopes.OrdersFull },
                // shared claims between all scopes
                UserClaims = { JwtClaimTypes.Role, ClaimsType.Permission },
            },
            new("gateway-api", "API Gateway")
            {
                // scope: what a client can do
                Scopes = { Scopes.Gateway },
            },
        };

    // - When a client asks for a scope (and that scope is allowed via configuration and not denied via consent), the value of that scope will be included in the resulting access token as a claim of type scope.
    // - The consumer of the access token can use that data to make sure that the client is actually allowed to invoke the corresponding functionality.
    // - Be aware that scopes are purely for authorizing clients, not users. In other words, the write scope allows the client to invoke the functionality associated with the scope and is unrelated to the user’s permission to do so.
    // https://docs.duendesoftware.com/identityserver/fundamentals/resources/api-scopes/
    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            // System scopes
            // Identity-related scopes (no associated ApiResource)
            new(Scopes.Roles, "User roles", [JwtClaimTypes.Role]),
            new(Scopes.UsersInfo, "User info", [ClaimsType.Type, ClaimsType.CustomAccess]),
            // Service scopes
            new(Scopes.Gateway, "API Gateway access", [JwtClaimTypes.Role, ClaimsType.Permission]),
            // catalogs
            new(Scopes.CatalogsRead, "Read access to catalogs"),
            new(Scopes.CatalogsWrite, "Write access to catalogs"),
            new(Scopes.CatalogsFull, "Full access to catalogs"),
            // users
            new(Scopes.UsersRead, "Read access to users"),
            new(Scopes.UsersWrite, "Write access to users"),
            new(Scopes.UsersFull, "Full access to users"),
            // customers
            new(Scopes.CustomersRead, "Read access to customers"),
            new(Scopes.CustomersWrite, "Write access to customers"),
            new(Scopes.CustomersFull, "Full access to customers"),
            // orders
            new(Scopes.OrdersRead, "Read access to orders"),
            new(Scopes.OrdersWrite, "Write access to orders"),
            new(Scopes.OrdersFull, "Full access to orders"),
        };

    // Clients configuration
    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            // SPA client using implicit flow (temporary)
            new()
            {
                ClientId = "spa",
                ClientName = "SPA Client",
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowAccessTokensViaBrowser = true,
                RequireClientSecret = false,
                RequireConsent = false,
                RedirectUris = { "https://localhost:5001/signin-oidc" },
                // - with using scope here, we specify this client can access to which scopes, regarding the user's permission, This prevents clients from requesting tokens for API areas they shouldn't access
                // - SPA might have catalogs:read scope, but a mobile app might only get catalogs:write
                // - First check: Does the client have the required scope? (Client authorization), Second check: Does the user have the required permissions? (User authorization)
                // - Scopes help limit the claims included in tokens (only include user claims relevant to the requested scopes)
                AllowedScopes =
                {
                    // we should use explicitly list individual scopes, not ApiResource names
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    // user-related scopes
                    Scopes.UsersInfo,
                    Scopes.Roles,
                    // gateway scope
                    Scopes.Gateway,
                    // catalogs scope
                    Scopes.CatalogsRead,
                    Scopes.CatalogsWrite,
                    Scopes.CatalogsFull,
                    // orders scopes
                    Scopes.OrdersRead,
                    Scopes.OrdersWrite,
                    Scopes.OrdersFull,
                    // customers scopes
                    Scopes.CustomersRead,
                    Scopes.CustomersWrite,
                    Scopes.CustomersFull,
                    // users scopes
                    Scopes.UsersRead,
                    Scopes.UsersWrite,
                    Scopes.UsersFull,
                },

                AccessTokenLifetime = 3600, // 1 hour
                IdentityTokenLifetime = 3600,
                AlwaysIncludeUserClaimsInIdToken = true,
            },
            new()
            {
                ClientId = "spa-pkce",
                ClientName = "SPA Client with PKCE",
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireClientSecret = false,
                AllowAccessTokensViaBrowser = true,
                RedirectUris = { "https://localhost:5001/signin-oidc" },
                PostLogoutRedirectUris = { "https://localhost:5001/signout-callback-oidc" },
                AllowedCorsOrigins = { "https://localhost:5001" },
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    Scopes.UsersInfo,
                    Scopes.Roles,
                    Scopes.Gateway,
                    Scopes.CatalogsRead,
                    Scopes.CatalogsWrite,
                    Scopes.CatalogsFull,
                    Scopes.OrdersRead,
                    Scopes.OrdersWrite,
                    Scopes.OrdersFull,
                    Scopes.CustomersRead,
                    Scopes.CustomersWrite,
                    Scopes.CustomersFull,
                    Scopes.UsersRead,
                    Scopes.UsersWrite,
                    Scopes.UsersFull,
                },
                AccessTokenLifetime = 3600,
                IdentityTokenLifetime = 3600,
                AlwaysIncludeUserClaimsInIdToken = true,
                RequireConsent = false,
            },
            // Gateway client (used for token exchange)
            new()
            {
                ClientId = "gateway",
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = { Scopes.CatalogsRead, Scopes.CatalogsWrite },
            },
        };
}
