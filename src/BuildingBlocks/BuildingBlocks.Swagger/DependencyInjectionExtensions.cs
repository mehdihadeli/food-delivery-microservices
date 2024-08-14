using BuildingBlocks.Core.Extensions.ServiceCollection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Unchase.Swashbuckle.AspNetCore.Extensions.Extensions;

namespace BuildingBlocks.Swagger;

public static class DependencyInjectionExtensions
{
    // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/README.md
    // https://github.com/dotnet/aspnet-api-versioning/tree/88323136a97a59fcee24517a514c1a445530c7e2/examples/AspNetCore/WebApi/MinimalOpenApiExample
    public static WebApplicationBuilder AddCustomSwagger(
        this WebApplicationBuilder builder,
        Action<SwaggerOptions>? configurator = null
    )
    {
        builder.Services.AddCustomSwagger(configurator);

        return builder;
    }

    public static IServiceCollection AddCustomSwagger(
        this IServiceCollection services,
        Action<SwaggerOptions>? configurator = null
    )
    {
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi
        services.AddEndpointsApiExplorer();

        services.TryAddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        services.AddValidatedOptions(configurator: configurator);

        services.AddSwaggerGen(options =>
        {
            // // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1269#issuecomment-577182931
            // options.SchemaFilter<EnumSchemaFilter>();

            // https://github.com/unchase/Unchase.Swashbuckle.AspNetCore.Extensions#fix-enums
            options.AddEnumsWithValuesFixFilters();

            options.OperationFilter<SwaggerDefaultValues>();
            options.OperationFilter<ApiVersionOperationFilter>();

            // https://github.com/domaindrivendev/Swashbuckle.AspNetCore#add-security-definitions-and-requirements
            // https://swagger.io/docs/specification/authentication/
            // https://medium.com/@niteshsinghal85/assign-specific-authorization-scheme-to-endpoint-in-swagger-ui-in-net-core-cd84d2a2ebd7
            var bearerScheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Name = JwtBearerDefaults.AuthenticationScheme,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                Reference = new() { Type = ReferenceType.SecurityScheme, Id = JwtBearerDefaults.AuthenticationScheme }
            };

            var apiKeyScheme = new OpenApiSecurityScheme
            {
                Description = "Api key needed to access the endpoints. X-Api-Key: My_API_Key",
                In = ParameterLocation.Header,
                Name = Constants.ApiKeyConstants.HeaderName,
                Scheme = Constants.ApiKeyConstants.DefaultScheme,
                Type = SecuritySchemeType.ApiKey,
                Reference = new() { Type = ReferenceType.SecurityScheme, Id = Constants.ApiKeyConstants.HeaderName }
            };

            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, bearerScheme);
            options.AddSecurityDefinition(Constants.ApiKeyConstants.HeaderName, apiKeyScheme);

            options.AddSecurityRequirement(
                new OpenApiSecurityRequirement
                {
                    { bearerScheme, Array.Empty<string>() },
                    { apiKeyScheme, Array.Empty<string>() }
                }
            );

            options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

            ////https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/467
            // options.OperationFilter<TagByApiExplorerSettingsOperationFilter>();
            // options.OperationFilter<TagBySwaggerOperationFilter>();

            // Enables Swagger annotations (SwaggerOperationAttribute, SwaggerParameterAttribute etc.)
            options.EnableAnnotations();
        });

        return services;
    }

    public static IApplicationBuilder UseCustomSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            var descriptions = app.DescribeApiVersions();

            // build a swagger endpoint for each discovered API version
            foreach (var description in descriptions)
            {
                var url = $"/swagger/{description.GroupName}/swagger.json";
                var name = description.GroupName.ToUpperInvariant();
                options.SwaggerEndpoint(url, name);
            }
        });

        return app;
    }
}
