using System.Reflection;
using BuildingBlocks.Core.Extensions.ServiceCollectionExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BuildingBlocks.OpenApi.Swashbuckle.Extensions;

public static class DependencyInjectionExtensions
{
    public static WebApplicationBuilder AddSwaggerOpenApi(this WebApplicationBuilder builder, Assembly appAssembly)
    {
        builder.Services.AddConfigurationOptions<OpenApiOptions>(nameof(OpenApiOptions));

        // only needed for minimal api but having it in controller based approach doesn't have any affect.
        builder.Services.AddEndpointsApiExplorer();

        // IConfigureOptions executes by the order that they are registered, here this configuration run before `AddSwaggerGen` configuration.
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

            AddSecurityScheme(options);
        });

        return builder;
    }

    private static void AddSecurityScheme(SwaggerGenOptions options)
    {
        // https://github.com/domaindrivendev/Swashbuckle.AspNetCore#add-security-definitions-and-requirements
        // Bearer token scheme
        options.AddSecurityDefinition(
            "Bearer",
            new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description =
                    "Enter 'Bearer' [space] and your token in the text input below.\n\nExample: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
            }
        );

        // API Key scheme
        options.AddSecurityDefinition(
            "ApiKey",
            new OpenApiSecurityScheme
            {
                Name = "X-API-KEY",
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Description = "Enter your API key in the text input below.\n\nExample: '12345-abcdef'",
            }
        );

        // Add Security Requirements
        options.AddSecurityRequirement(
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                    },
                    new List<string>() // No specific scopes for bearer
                },
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" },
                    },
                    new List<string>() // No specific scopes for API Key
                },
            }
        );
    }
}
