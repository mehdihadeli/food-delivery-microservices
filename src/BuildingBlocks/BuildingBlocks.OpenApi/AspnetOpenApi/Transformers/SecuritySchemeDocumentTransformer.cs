using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using OAuthOptions = BuildingBlocks.Core.Security.OAuthOptions;

namespace BuildingBlocks.OpenApi.AspnetOpenApi.Transformers;

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/customize-openapi?view=aspnetcore-9.0#customize-openapi-documents-with-transformers
public class SecuritySchemeDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        var openApiOptions = context.ApplicationServices.GetRequiredService<IOptions<OpenApiOptions>>();

        document.Components ??= new();

        switch (openApiOptions.Value.SecurityUIMode)
        {
            case SecurityUIMode.Oauth2:
                var oauthOptions = context.ApplicationServices.GetRequiredService<IOptions<OAuthOptions>>();
                var scopesDictionary = oauthOptions.Value.OpenApiScopes.ToDictionary(
                    scope => scope,
                    scope => $"Access to {scope}"
                );

                var oauthSecurityScheme = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Implicit = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri($"{oauthOptions.Value.Authority}/connect/authorize"),
                            TokenUrl = new Uri($"{oauthOptions.Value.Authority}/connect/token"),
                            Scopes = scopesDictionary,
                        },
                        // Authorization Code flow with PKCE
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri($"{oauthOptions.Value.Authority}/connect/authorize"),
                            TokenUrl = new Uri($"{oauthOptions.Value.Authority}/connect/token"),
                            Scopes = scopesDictionary,
                        },
                    },
                };
                document.Components.SecuritySchemes.Add(OAuthDefaults.DisplayName, oauthSecurityScheme);
                break;
            case SecurityUIMode.ApiKey:
                // API Key scheme
                var apiKeySecurityScheme = new OpenApiSecurityScheme
                {
                    Name = "X-API-KEY",
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header,
                    Description = "Enter your API key in the text input below.\n\nExample: '12345-abcdef'",
                };
                document.Components.SecuritySchemes.Add("ApiKey", apiKeySecurityScheme);
                break;
            case SecurityUIMode.Jwt:
                // Bearer token scheme
                var bearerSecurityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description =
                        "Enter 'Bearer' [space] and your token in the text input below.\n\nExample: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
                };
                document.Components.SecuritySchemes.Add("Bearer", bearerSecurityScheme);
                break;
        }

        return Task.CompletedTask;
    }
}
