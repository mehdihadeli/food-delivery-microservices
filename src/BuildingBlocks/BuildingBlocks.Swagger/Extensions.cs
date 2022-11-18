using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BuildingBlocks.Swagger;

public static class Extensions
{
    // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/README.md
    // https://github.com/dotnet/aspnet-api-versioning/tree/88323136a97a59fcee24517a514c1a445530c7e2/examples/AspNetCore/WebApi/MinimalOpenApiExample
    public static WebApplicationBuilder AddCustomSwagger(this WebApplicationBuilder builder, Assembly assembly)
    {
        builder.Services.AddCustomSwagger(builder.Configuration, assembly);

        return builder;
    }

    public static IServiceCollection AddCustomSwagger(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly assembly)
    {
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi
        services.AddEndpointsApiExplorer();

        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        services.AddOptions<SwaggerOptions>().Bind(configuration.GetSection(nameof(SwaggerOptions)))
            .ValidateDataAnnotations();

        services.AddSwaggerGen(
            options =>
            {
                options.OperationFilter<SwaggerDefaultValues>();
                options.OperationFilter<ApiVersionOperationFilter>();

                var xmlFile = XmlCommentsFilePath(assembly);
                if (File.Exists(xmlFile)) options.IncludeXmlComments(xmlFile);

                // https://github.com/domaindrivendev/Swashbuckle.AspNetCore#add-security-definitions-and-requirements
                // https://swagger.io/docs/specification/authentication/
                // https://medium.com/@niteshsinghal85/assign-specific-authorization-scheme-to-endpoint-in-swagger-ui-in-net-core-cd84d2a2ebd7
                var bearerScheme = new OpenApiSecurityScheme()
                {
                    Type = SecuritySchemeType.Http,
                    Name = JwtBearerDefaults.AuthenticationScheme,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    Reference = new()
                    {
                        Type = ReferenceType.SecurityScheme, Id = JwtBearerDefaults.AuthenticationScheme
                    }
                };

                var apiKeyScheme = new OpenApiSecurityScheme
                {
                    Description = "Api key needed to access the endpoints. X-Api-Key: My_API_Key",
                    In = ParameterLocation.Header,
                    Name = Constants.ApiKeyConstants.HeaderName,
                    Scheme = Constants.ApiKeyConstants.DefaultScheme,
                    Type = SecuritySchemeType.ApiKey,
                    Reference = new()
                    {
                        Type = ReferenceType.SecurityScheme, Id = Constants.ApiKeyConstants.HeaderName
                    }
                };

                options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, bearerScheme);
                options.AddSecurityDefinition(Constants.ApiKeyConstants.HeaderName, apiKeyScheme);

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {bearerScheme, Array.Empty<string>()}, {apiKeyScheme, Array.Empty<string>()}
                });

                options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

                ////https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/467
                // options.OperationFilter<TagByApiExplorerSettingsOperationFilter>();
                // options.OperationFilter<TagBySwaggerOperationFilter>();

                // Enables Swagger annotations (SwaggerOperationAttribute, SwaggerParameterAttribute etc.)
                options.EnableAnnotations();
            });

        static string XmlCommentsFilePath(Assembly assembly)
        {
            var basePath = Path.GetDirectoryName(assembly.Location);
            var fileName = assembly.GetName().Name + ".xml";
            return Path.Combine(basePath, fileName);
        }

        return services;
    }

    public static IApplicationBuilder UseCustomSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(
            options =>
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
