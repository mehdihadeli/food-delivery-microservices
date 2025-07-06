using System.Reflection;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollectionExtensions;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using OAuthOptions = BuildingBlocks.Core.Security.OAuthOptions;

namespace BuildingBlocks.OpenApi.Swashbuckle.Extensions;

public static class DependencyInjectionExtensions
{
    public static IHostApplicationBuilder AddSwaggerOpenApi(this IHostApplicationBuilder builder, Assembly appAssembly)
    {
        builder.Services.AddConfigurationOptions<OpenApiOptions>();
        var openApiOptions = builder.Configuration.BindOptions<OpenApiOptions>();
        var oAuthOptions = builder.Configuration.BindOptions<OAuthOptions>();

        // only needed for minimal api but having it in a controller-based approach doesn't have any effect.
        builder.Services.AddEndpointsApiExplorer();

        // IConfigureOptions executes by the order that they are registered, here this configuration runs before `AddSwaggerGen` configuration.
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-9.0#options-interfaces
        builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerGenVersioningOptions>();

        builder.Services.AddSwaggerGen(options =>
        {
            // https://github.com/domaindrivendev/Swashbuckle.AspNetCore#customize-schema-ids
            options.CustomSchemaIds(type => type.FullName);
            options.CustomOperationIds(apiDesc =>
                apiDesc.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null
            );

            // https://github.com/domaindrivendev/Swashbuckle.AspNetCore#extend-generator-with-operation-schema--document-filters
            // add a custom operation filter which sets default values
            options.OperationFilter<CorrelationIdHeaderOperationFilter>();
            options.OperationFilter<SwaggerDefaultValuesOperationFilter>();
            options.SchemaFilter<EnumSchemaFilter>();

            // https://github.com/domaindrivendev/Swashbuckle.AspNetCore#swashbuckleaspnetcoreannotations
            options.EnableAnnotations();

            var fileName = $"{appAssembly.GetName().Name}.xml";
            var filePath = Path.Combine(AppContext.BaseDirectory, fileName);

            // swagger supports new metadata attributes for generating open-api in .net 9 as well when we don't want to use xml for generating open-api
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/include-metadata?view=aspnetcore-9.0&tabs=controllers#use-attributes-to-add-metadata
            if (File.Exists(filePath))
            {
                // https://github.com/domaindrivendev/Swashbuckle.AspNetCore#include-descriptions-from-xml-comments
                options.IncludeXmlComments(filePath);
            }

            AddSecurityScheme(options, openApiOptions, oAuthOptions);
        });

        return builder;
    }

    private static void AddSecurityScheme(
        SwaggerGenOptions options,
        OpenApiOptions openApiOptions,
        OAuthOptions oauthOptions
    )
    {
        // https://github.com/domaindrivendev/Swashbuckle.AspNetCore#add-security-definitions-and-requirements
        switch (openApiOptions.SecurityUIMode)
        {
            case SecurityUIMode.Oauth2:
                var scopesDictionary = oauthOptions.OpenApiScopes.ToDictionary(
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
                            AuthorizationUrl = new Uri($"{oauthOptions.Authority}/connect/authorize"),
                            TokenUrl = new Uri($"{oauthOptions.Authority}/connect/token"),
                            Scopes = scopesDictionary,
                        },
                        // Authorization Code flow with PKCE
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri($"{oauthOptions.Authority}/connect/authorize"),
                            TokenUrl = new Uri($"{oauthOptions.Authority}/connect/token"),
                            Scopes = scopesDictionary,
                        },
                    },
                };
                options.AddSecurityDefinition(OAuthDefaults.DisplayName, oauthSecurityScheme);
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
                options.AddSecurityDefinition("ApiKey", apiKeySecurityScheme);
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
                options.AddSecurityDefinition("Bearer", bearerSecurityScheme);
                break;
        }
    }
}
