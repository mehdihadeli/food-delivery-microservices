using BuildingBlocks.Core.Security;
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
public static class Config
{
    // Identity Resources (for ID token)
    public static IEnumerable<IdentityResource> GetIdentityResources() =>
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
    public static IEnumerable<ApiResource> GetApiResources() =>
        // - The `combination` of allowed scopes (what the client is permitted to do) and user permissions (what the user is permitted to do) ensures security.
        new List<ApiResource>
        {
            new("catalogs-api", "Catalogs API")
            {
                // what client can do
                Scopes =
                {
                    Authorization.Scopes.CatalogsRead,
                    Authorization.Scopes.CatalogsWrite,
                    Authorization.Scopes.CatalogsFull,
                },
                // shared claims between all scopes
                UserClaims = { JwtClaimTypes.Role, ClaimsType.Permission },
            },
            new("customers-api", "Customers API")
            {
                // what client can do
                Scopes =
                {
                    Authorization.Scopes.CustomersRead,
                    Authorization.Scopes.CustomersWrite,
                    Authorization.Scopes.CustomersFull,
                },
                // shared claims between all scopes
                UserClaims = { JwtClaimTypes.Role, ClaimsType.Permission },
            },
            new("users-api", "Users API")
            {
                // what client can do
                Scopes =
                {
                    Authorization.Scopes.UsersRead,
                    Authorization.Scopes.UsersWrite,
                    Authorization.Scopes.UsersFull,
                },
                // shared claims between all scopes
                UserClaims = { JwtClaimTypes.Role, ClaimsType.Permission },
            },
            new("orders-api", "Orders API")
            {
                // what client can do
                Scopes =
                {
                    Authorization.Scopes.OrdersRead,
                    Authorization.Scopes.OrdersWrite,
                    Authorization.Scopes.OrdersFull,
                },
                // shared claims between all scopes
                UserClaims = { JwtClaimTypes.Role, ClaimsType.Permission },
            },
            new("gateway-api", "API Gateway")
            {
                // scope: what a client can do
                Scopes = { Authorization.Scopes.Gateway },
            },
        };

    // - When a client asks for a scope (and that scope is allowed via configuration and not denied via consent), the value of that scope will be included in the resulting access token as a claim of type scope.
    // - The consumer of the access token can use that data to make sure that the client is actually allowed to invoke the corresponding functionality.
    // - Be aware that scopes are purely for authorizing clients, not users. In other words, the write scope allows the client to invoke the functionality associated with the scope and is unrelated to the user’s permission to do so.
    // https://docs.duendesoftware.com/identityserver/fundamentals/resources/api-scopes/
    public static IEnumerable<ApiScope> GetApiScopes() =>
        new List<ApiScope>
        {
            // System scopes
            // Identity-related scopes (no associated ApiResource)
            new(Authorization.Scopes.Roles, "User roles", [JwtClaimTypes.Role]),
            new(Authorization.Scopes.UsersInfo, "User info", [ClaimsType.Type, ClaimsType.CustomAccess]),
            // Service scopes
            new(Authorization.Scopes.Gateway, "API Gateway access", [JwtClaimTypes.Role, ClaimsType.Permission]),
            // catalogs
            new(Authorization.Scopes.CatalogsRead, "Read access to catalogs"),
            new(Authorization.Scopes.CatalogsWrite, "Write access to catalogs"),
            new(Authorization.Scopes.CatalogsFull, "Full access to catalogs"),
            // users
            new(Authorization.Scopes.UsersRead, "Read access to users"),
            new(Authorization.Scopes.UsersWrite, "Write access to users"),
            new(Authorization.Scopes.UsersFull, "Full access to users"),
            // customers
            new(Authorization.Scopes.CustomersRead, "Read access to customers"),
            new(Authorization.Scopes.CustomersWrite, "Write access to customers"),
            new(Authorization.Scopes.CustomersFull, "Full access to customers"),
            // orders
            new(Authorization.Scopes.OrdersRead, "Read access to orders"),
            new(Authorization.Scopes.OrdersWrite, "Write access to orders"),
            new(Authorization.Scopes.OrdersFull, "Full access to orders"),
        };

    // Clients configuration
    public static IEnumerable<Client> GetClients(IConfiguration configuration) =>
        // https://github.com/DuendeSoftware/demo.duendesoftware.com
        // https://demo.duendesoftware.com/

        // https://docs.duendesoftware.com/bff/
        // - Duende.BFF is designed to secure browser-based frontends (SPAs, Blazor WASM) needing cookie-based token management. Native apps (MAUI) use mobile-specific auth flows (PKCE) and don’t need cookie orchestration.
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
                RedirectUris = { $"{configuration["ReactSpaAddress"]}/signin-oidc" },
                // - with using scope here, we specify this client can access to which scopes, regarding the user's permission, This prevents clients from requesting tokens for API areas they shouldn't access
                // - SPA might have catalogs:read scope, but a mobile app might only get catalogs:write
                // - First check: Does the client have the required scope? (Client authorization), Second check: Does the user have the required permissions? (User authorization)
                // - Scopes help limit the claims included in tokens (only include user claims relevant to the requested scopes)
                AllowedScopes =
                {
                    // implicit-flow doesn't have an id-token so we don't have identity scopes

                    // user-related scopes
                    Authorization.Scopes.UsersInfo,
                    Authorization.Scopes.Roles,
                    // gateway scope
                    Authorization.Scopes.Gateway,
                    // catalogs scope
                    Authorization.Scopes.CatalogsRead,
                    Authorization.Scopes.CatalogsWrite,
                    Authorization.Scopes.CatalogsFull,
                    // orders scopes
                    Authorization.Scopes.OrdersRead,
                    Authorization.Scopes.OrdersWrite,
                    Authorization.Scopes.OrdersFull,
                    // customers scopes
                    Authorization.Scopes.CustomersRead,
                    Authorization.Scopes.CustomersWrite,
                    Authorization.Scopes.CustomersFull,
                    // users scopes
                    Authorization.Scopes.UsersRead,
                    Authorization.Scopes.UsersWrite,
                    Authorization.Scopes.UsersFull,
                },

                AccessTokenLifetime = 3600, // 1 hour
                IdentityTokenLifetime = 3600,
                AlwaysIncludeUserClaimsInIdToken = true,
            },
            // - Authorization Code Flow - When using a BFF (Backend for Frontend) architecture with an SPA, the Authorization Code Flow works differently than in a pure SPA (which requires PKCE).
            // - The BFF is a confidential client (runs on your server), so it can securely store a ClientSecret. The SPA (browser) never sees tokens or exchanges codes—it delegates all auth to the BFF.
            // - 1) SPA (browser) → Redirects to auth server via BFF. 2) Auth Server → Returns auth code to BFF (not the SPA directly). 3) BFF → Exchanges code for tokens (using ClientSecret). 4) BFF → Stores tokens securely (HTTP-only cookies) and proxies API calls.
            // - PKCE Not needed, because The BFF is a server-side component (not a public client). The ClientSecret protects against public access.
            new()
            {
                ClientId = "spa-bff-code-flow",
                ClientName = "SPA Client with Code Flow",
                AllowedGrantTypes = GrantTypes.Code,
                // PKCE protects the authorization code (front-channel) using PKCE parameters (code_verifier, code_challenge, code_challenge_method)
                // if we set `PKCE=true` PKCE parameters (code_verifier, code_challenge, code_challenge_method) are mandatory for more protection for the front (/authorize) and back channel (connect/token) and we get bad request otherwise they are optional
                RequirePkce = true,
                AllowOfflineAccess = true,
                AllowAccessTokensViaBrowser = false,
                // make client secret mandatory, for more protection on backend channel (connect/token)
                RequireClientSecret = true,
                // - Client secret protects the `connect/token` endpoint (back-channel)
                // - when we enforce client_secrets only authorized clients can exchange codes, otherwise anyone with a stolen `auth code` can get tokens
                // - anyone needs both `code + client_secret + PKCE verifier` to get tokens but without enforcing client_secret anyone only needs the `code + PKCE verifier` to get the token.
                // - If the client is a public client (e.g., SPA, mobile app) → Must use PKCE (code_verifier) instead of `client_secret` which can't be public secret to increase security
                ClientSecrets = { new Secret("secret".Sha256()) },
                // - We redirect to our spa or gateway then using a reverse proxy route to our bff to handle the oidc callback operations
                // - After registering authentication scheme handlers like `AddOpenIdConnect` and `AddCookie` and specifying default `DefaultChallengeScheme` with `AddAuthentication`, scheme authentication handlers will use later in bff endpoint handlers or authentication or authorization middlewares,
                // when we call our bff with `bff/login` request, because we registered bff endpoints with `MapBffManagementEndpoints` our bff login endpoint handler which is DefaultLoginService will be called and inside handler it calls `HttpContext.ChallengeAsync` which is actually calling ChallengeAsync
                // on `AuthenticationService` which is added by `AddAuthentication` and then get default registered challenge scheme for authentication which is `OpenIdConnectHandler` and call `ChallengeAsync` on it and the calling HandleChallengeAsyncInternal,
                // then if RequirePkce=true && Options.ResponseType == OpenIdConnectResponseType.Code it create `code_challenge`, `code_verifier` adn `code_challenge_method=S256`. then based on OpneIdConnectOptions it creates OpenIdConnectMessage and for `OpenIdConnectMessage.RedirectUri` will set by
                //  Request.Scheme (our spa scheme) + Uri.SchemeDelimiter + Request.Host (spa host) + OpneIdConnectOptions.CallbackPath which is `signin-oidc`, the get OpenIdConnectConfiguration using `OpneIdConnectOptions.ConfigurationManager.GetConfigurationAsync` which contains identity server metadata and
                // well-known information and after that set `OpenIdConnectMessage.IssuerAddress=OpenIdConnectConfiguration?.AuthorizationEndpoint` (https://localhost:7001/connect/authorize), also create a properties containing `RedirectUrl=https://localhost:5173/signin-oidc` and `code_verifier`.
                // now `OpenIdConnectMessage.CreateAuthenticationRequestUrl` method uses `OpenIdConnectMessage.IssuerAddress` and parameters to generate a front channel authorize endpoint (https://localhost:7001/connect/authorize?client_id=spa-bff-code-flow&...) for getting `code` in pkce, and by calling
                // this authorize endpoint redirect user to identity server login page for generating a `code` and redirecting code challenge to `OpenIdConnectMessage.RedirectUri=https://localhost:5173/signin-oidc` endpoint which our front end that should re-route the request to bff using reverse proxy.
                // then Authentication middleware on bff will be called and run register handler like OpenIdConnectHandler and run `HandleRequestAsync` on it, now inside HandleRequestAsync because `ShouldHandleRequestAsync` now is true because
                // OpenIdConnectOption.CallbackPath == Request.Path and we get a request from frontend and reverse proxy (https://localhost:5173/signin-oidc) and its `Path` is `signin-oidc` equal to `OpenIdConnectOption.CallbackPath` which is as default `signin-oidc` so we skip
                // ShouldHandleRequestAsync() condition in `ShouldHandleRequestAsync` and go further, when our user request doesn't have `signin-oidc` the handler return immediately. then it calls HandleRemoteAuthenticateAsync and here we get `code` and passed parameters liked code_verifier and
                // because our request method is Get it creates a `OpenIdConnectMessage` from request query string like `scope`, `code` and `state` and also read parameters from State with `ReadPropertiesAndClearState` and create a `AuthenticationProperties` and contains `code_verifier` and `redirect` and then
                // run RunMessageReceivedEventAsync with passing `OpenIdConnectMessage` and `AuthenticationProperties` and create a `MessageRecivedContext` and then if `code` is not null run `RunAuthorizationCodeReceivedEventAsync` with passing `OpenIdConnectMessage` and `AuthenticationProperties` and create a new
                // `OpenIdConnectMessage` for second backend channel flow using `connect/token` and create `AuthorizationCodeReceivedContext` with new OpenIdConnectMessage for backend channel to get token. using `RedeemAuthorizationCodeAsync` and passing new `OpenIdConnectMessage` create a HttpRequestMessage with type post
                // and get `connect/token` uri identity server from OpenIdConnectConfiguration using `Options.ConfigurationManager.GetConfigurationAsync` and passing `OpenIdConnectMessage` Parameters like `code`, `client_secret`, `code_verifier` to create a `FormUrlEncodedContent` and set  RequestMessage.Content with this Content.
                // after getting response from httpclient create a `OpenIdConnectMessage` from response which contains `access_token`, `id_token` and `refresh_token` and then run `RunTokenResponseReceivedEventAsync` with passing response `OpenIdConnectMessage` and create `ClaimsPrincipal` using `ValidateTokenUsingHandlerAsync` and thrn
                // call `RunTokenValidatedEventAsync` and create a `TokenValidatedContext` and at the end if OpenIdConnectOptions.GetClaimsFromUserInfoEndpoint=true it call `GetUserInformationAsync` and returns `HandleRequestResult` which internally uses `https://localhost:7001/connect/userinfo` identity server endpoint and return
                // a HandleRequestResult containing `AuthenticationTicket` and schema name `OpenIdConnect` and save token in the cookie session on bff. after that in our spa we can call `bff/user` to get user claims and duende.bff user endpoint handle request with `DefaultUserService` and if a user is authenticated return user claims.

                // - default callback urls for OpenIdConnectOptions constructor and AddOpenIdConnect middleware which are `OpenIdConnectOptions.CallbackPath=/signin-oidc`, `OpenIdConnectOptions.SignedOutCallbackPath=/signout-callback-oidc`, `OpenIdConnectOptions.RemoteSignOutPath=/signout-oidc`
                // - `OpenIdConnectHandler` and its `HandleChallengeAsyncInternal` handle first phase code flow to redirect to login page
                // - `OpenIdConnectHandler` and its `HandleRemoteAuthenticateAsync` handles callback request for `signin-oidc` after login, `/signout-callback-oidc` after logout,... through having `AddOpenIdConnect` in our bff for completing second phase in code flow for validating `code` challenge and send request for getting token
                RedirectUris = { $"{configuration["ReactSpaAddress"]}/signin-oidc" },
                FrontChannelLogoutUri = $"{configuration["ReactSpaAddress"]}/signout-oidc",
                PostLogoutRedirectUris = { $"{configuration["ReactSpaAddress"]}/signout-callback-oidc" },
                AllowedScopes =
                {
                    // id-token is available in pkce and is not available in implicit-low
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.OfflineAccess,
                    Authorization.Scopes.UsersInfo,
                    Authorization.Scopes.Roles,
                    Authorization.Scopes.Gateway,
                    Authorization.Scopes.CatalogsRead,
                    Authorization.Scopes.CatalogsWrite,
                    Authorization.Scopes.CatalogsFull,
                    Authorization.Scopes.OrdersRead,
                    Authorization.Scopes.OrdersWrite,
                    Authorization.Scopes.OrdersFull,
                    Authorization.Scopes.CustomersRead,
                    Authorization.Scopes.CustomersWrite,
                    Authorization.Scopes.CustomersFull,
                    Authorization.Scopes.UsersRead,
                    Authorization.Scopes.UsersWrite,
                    Authorization.Scopes.UsersFull,
                },
                AccessTokenLifetime = 60 * 60 * 2, // 2 hours
                IdentityTokenLifetime = 60 * 60 * 2, // 2 hours
                AlwaysIncludeUserClaimsInIdToken = true,
                RequireConsent = false,
            },
            // Blazor WebApp run on the server, so they are confidential clients.They can securely store a client secret, unlike SPAs.
            // There's no need for PKCE, which is designed for public clients (e.g., JavaScript apps). Even though this is a confidential client, enabling PKCE adds defense-in-depth.
            // The server handles the entire redirect and token exchange process, keeping tokens secure.
            new()
            {
                ClientId = "blazor-webapp-code-flow",
                ClientName = "Blazor WebApp client with Code Flow",
                AllowedGrantTypes = GrantTypes.Code,
                // PKCE protects the authorization code (front-channel) using PKCE parameters (code_verifier, code_challenge, code_challenge_method)
                // if we set `PKCE=true` PKCE parameters (code_verifier, code_challenge, code_challenge_method) are mandatory for more protection for the front (/authorize) and back channel (connect/token) and we get bad request otherwise they are optional
                RequirePkce = true, // Not needed for server-side apps
                RequireClientSecret = true, // Required for a confidential client
                ClientSecrets = { new Secret("secret".Sha256()) },
                RedirectUris = { $"{configuration["BlazorWebAppAddress"]}/signin-oidc" },
                FrontChannelLogoutUri = $"{configuration["BlazorWebAppAddress"]}/signout-oidc",
                PostLogoutRedirectUris = { $"{configuration["BlazorWebAppAddress"]}/signout-callback-oidc" },
                AllowedScopes =
                {
                    // id-token is available in pkce and is not available in implicit-low
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.OfflineAccess,
                    Authorization.Scopes.UsersInfo,
                    Authorization.Scopes.Roles,
                    Authorization.Scopes.Gateway,
                    Authorization.Scopes.CatalogsRead,
                    Authorization.Scopes.CatalogsWrite,
                    Authorization.Scopes.CatalogsFull,
                    Authorization.Scopes.OrdersRead,
                    Authorization.Scopes.OrdersWrite,
                    Authorization.Scopes.OrdersFull,
                    Authorization.Scopes.CustomersRead,
                    Authorization.Scopes.CustomersWrite,
                    Authorization.Scopes.CustomersFull,
                    Authorization.Scopes.UsersRead,
                    Authorization.Scopes.UsersWrite,
                    Authorization.Scopes.UsersFull,
                },
                AllowOfflineAccess = true,
                AllowAccessTokensViaBrowser = false,
                RequireConsent = false,
            },
            new()
            {
                ClientId = "test-client-credential",
                ClientName = "Test Client Credentials",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedScopes =
                {
                    // id-token is available in pkce and is not available in implicit-low
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.OfflineAccess,
                    Authorization.Scopes.UsersInfo,
                    Authorization.Scopes.Roles,
                    Authorization.Scopes.Gateway,
                    Authorization.Scopes.CatalogsRead,
                    Authorization.Scopes.CatalogsWrite,
                    Authorization.Scopes.CatalogsFull,
                    Authorization.Scopes.OrdersRead,
                    Authorization.Scopes.OrdersWrite,
                    Authorization.Scopes.OrdersFull,
                    Authorization.Scopes.CustomersRead,
                    Authorization.Scopes.CustomersWrite,
                    Authorization.Scopes.CustomersFull,
                    Authorization.Scopes.UsersRead,
                    Authorization.Scopes.UsersWrite,
                    Authorization.Scopes.UsersFull,
                },
            },
            new()
            {
                ClientId = "maui",
                ClientName = "MAUI Client",
                AllowedGrantTypes = GrantTypes.Code,
                RedirectUris = { configuration["MauiBffAddress"] },
                // Remove ClientSecrets (MAUI is a public client)
                RequireConsent = false,
                RequirePkce = true, // mobile app is public and pkce should be true
                PostLogoutRedirectUris = { $"{configuration["MauiBffAddress"]}/Account/Redirecting" },
                AllowedScopes =
                {
                    // id-token is available in pkce and is not available in implicit-low
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.OfflineAccess,
                    Authorization.Scopes.UsersInfo,
                    Authorization.Scopes.Roles,
                    Authorization.Scopes.Gateway,
                    Authorization.Scopes.CatalogsRead,
                    Authorization.Scopes.CatalogsWrite,
                    Authorization.Scopes.CatalogsFull,
                    Authorization.Scopes.OrdersRead,
                    Authorization.Scopes.OrdersWrite,
                    Authorization.Scopes.OrdersFull,
                    Authorization.Scopes.CustomersRead,
                    Authorization.Scopes.CustomersWrite,
                    Authorization.Scopes.CustomersFull,
                    Authorization.Scopes.UsersRead,
                    Authorization.Scopes.UsersWrite,
                    Authorization.Scopes.UsersFull,
                },
                // Allow requesting refresh tokens for long lived API access
                AllowOfflineAccess = true,
                // Needed for deep-link redirects
                AllowAccessTokensViaBrowser = true,
                AlwaysIncludeUserClaimsInIdToken = true,
                AccessTokenLifetime = 60 * 60 * 2, // 2 hours
                IdentityTokenLifetime = 60 * 60 * 2, // 2 hours
            },
            new()
            {
                ClientId = "identity-swaggerui",
                ClientName = "Identity Swagger UI",
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowAccessTokensViaBrowser = true,
                RedirectUris = { $"{configuration["IdentityAddress"]}/swagger/oauth2-redirect.html" },
                PostLogoutRedirectUris = { $"{configuration["IdentityAddress"]}/swagger/" },
                AllowedScopes =
                {
                    Authorization.Scopes.UsersInfo,
                    Authorization.Scopes.Roles,
                    Authorization.Scopes.Gateway,
                    Authorization.Scopes.UsersRead,
                    Authorization.Scopes.UsersWrite,
                    Authorization.Scopes.UsersFull,
                },
            },
            new()
            {
                ClientId = "catalogs-swaggerui",
                ClientName = "Catalogs Swagger UI",
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowAccessTokensViaBrowser = true,
                RedirectUris = { $"{configuration["CatalogsAddress"]}/swagger/oauth2-redirect.html" },
                PostLogoutRedirectUris = { $"{configuration["CatalogsAddress"]}/swagger/" },
                AllowedScopes =
                {
                    Authorization.Scopes.UsersInfo,
                    Authorization.Scopes.Roles,
                    Authorization.Scopes.Gateway,
                    Authorization.Scopes.CatalogsRead,
                    Authorization.Scopes.CatalogsWrite,
                    Authorization.Scopes.CatalogsFull,
                },
            },
            new()
            {
                ClientId = "customers-swaggerui",
                ClientName = "Customers Swagger UI",
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowAccessTokensViaBrowser = true,
                RedirectUris = { $"{configuration["CustomersAddress"]}/swagger/oauth2-redirect.html" },
                PostLogoutRedirectUris = { $"{configuration["CustomersAddress"]}/swagger/" },
                AllowedScopes =
                {
                    Authorization.Scopes.UsersInfo,
                    Authorization.Scopes.Roles,
                    Authorization.Scopes.Gateway,
                    Authorization.Scopes.CustomersRead,
                    Authorization.Scopes.CustomersWrite,
                    Authorization.Scopes.CustomersFull,
                },
            },
            new()
            {
                ClientId = "orders-swaggerui",
                ClientName = "Orders Swagger UI",
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowAccessTokensViaBrowser = true,
                RedirectUris = { $"{configuration["OrdersAddress"]}/swagger/oauth2-redirect.html" },
                PostLogoutRedirectUris = { $"{configuration["OrdersAddress"]}/swagger/" },
                AllowedScopes =
                {
                    Authorization.Scopes.UsersInfo,
                    Authorization.Scopes.Roles,
                    Authorization.Scopes.Gateway,
                    Authorization.Scopes.OrdersRead,
                    Authorization.Scopes.OrdersWrite,
                    Authorization.Scopes.OrdersFull,
                },
            },
        };
}
