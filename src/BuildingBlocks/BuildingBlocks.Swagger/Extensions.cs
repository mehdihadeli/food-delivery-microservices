using System.Reflection;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace BuildingBlocks.Swagger;

public static class Extensions
{
    // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/README.md
    public static WebApplicationBuilder AddCustomSwagger(
        this WebApplicationBuilder builder,
        IConfiguration configuration,
        Assembly assembly)
    {
        builder.Services.AddCustomSwagger(configuration, assembly);

        return builder;
    }

    public static IServiceCollection AddCustomSwagger(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly assembly,
        bool useApiVersioning = false)
    {
        // swagger docs for route to code style --> works in .net 6
        // https://dotnetthoughts.net/openapi-support-for-aspnetcore-minimal-webapi/
        // https://jaliyaudagedara.blogspot.com/2021/07/net-6-preview-6-introducing-openapi.html
        services.AddEndpointsApiExplorer();

        services.AddOptions<SwaggerOptions>().Bind(configuration.GetSection(nameof(SwaggerOptions)))
            .ValidateDataAnnotations();

        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

        services.AddSwaggerGen(
            options =>
            {
                options.OperationFilter<SwaggerDefaultValues>();
                options.OperationFilter<ApiVersionOperationFilter>();

                var xmlFile = XmlCommentsFilePath(assembly);
                if (File.Exists(xmlFile)) options.IncludeXmlComments(xmlFile);

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n
                              Enter 'Bearer' [space] and then your token in the text input below.
                              \r\n\r\nExample: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityDefinition(
                    Constants.ApiKeyConstants.HeaderName,
                    new OpenApiSecurityScheme
                    {
                        Description = "Api key needed to access the endpoints. X-Api-Key: My_API_Key",
                        In = ParameterLocation.Header,
                        Name = Constants.ApiKeyConstants.HeaderName,
                        Type = SecuritySchemeType.ApiKey
                    });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme, Id = "Bearer"},
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    },
                    {
                        new OpenApiSecurityScheme
                        {
                            Name = Constants.ApiKeyConstants.HeaderName,
                            Type = SecuritySchemeType.ApiKey,
                            In = ParameterLocation.Header,
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme, Id = Constants.ApiKeyConstants.HeaderName
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

        services.Configure<SwaggerGeneratorOptions>(o => o.InferSecuritySchemes = true);

        return services;

        static string XmlCommentsFilePath(Assembly assembly)
        {
            var basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var fileName = assembly.GetName().Name + ".xml";
            return Path.Combine(basePath, fileName);
        }
    }

    public static IApplicationBuilder UseCustomSwagger(
        this IApplicationBuilder app,
        IApiVersionDescriptionProvider? provider = null)
    {
        app.UseSwagger();
        app.UseSwaggerUI(
            options =>
            {
                options.DocExpansion(DocExpansion.None);
                if (provider is null)
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "API");
                }
                else
                {
                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        options.SwaggerEndpoint(
                            $"/swagger/{description.GroupName}/swagger.json",
                            description.GroupName.ToUpperInvariant());
                    }
                }
            });

        return app;
    }
}
