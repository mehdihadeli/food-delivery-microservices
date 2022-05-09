using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
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
        if (useApiVersioning)
        {
            services.AddVersionedApiExplorer(options =>
            {
                // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                // note: the specified format code will format the version as "'v'major[.minor][-status]"
                options.GroupNameFormat = "'v'VVV";

                // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                // can also be used to control the format of the API version in route templates
                options.SubstituteApiVersionInUrl = true;
            });
        }

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
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
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

                if (useApiVersioning)
                {
                    // Grouping endpoints by version and ApiExplorer group name.
                    options.DocInclusionPredicate((documentName, apiDescription) =>
                    {
                        var actionApiVersionModel = apiDescription.ActionDescriptor
                            .GetApiVersionModel(ApiVersionMapping.Explicit | ApiVersionMapping.Implicit);

                        var apiExplorerSettingsAttribute =
                            (ApiExplorerSettingsAttribute)apiDescription.ActionDescriptor
                                .EndpointMetadata.FirstOrDefault(x =>
                                    x.GetType() == typeof(ApiExplorerSettingsAttribute))!;

                        if (apiExplorerSettingsAttribute == null) return true;

                        if (actionApiVersionModel.DeclaredApiVersions.Any())
                        {
                            return actionApiVersionModel.DeclaredApiVersions.Any(v =>
                                $"v{v.MajorVersion}" == documentName);
                        }

                        return actionApiVersionModel.ImplementedApiVersions.Any(v =>
                            $"v{v.MajorVersion}" == documentName);
                    });

                    // Adding all the available versions.
                    var apiVersionDescriptionProvider = services.BuildServiceProvider()
                        .GetService<IApiVersionDescriptionProvider>();

                    if (apiVersionDescriptionProvider != null)
                    {
                        foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                        {
                            var openApiInfo = new OpenApiInfo
                            {
                                Title = $"{description.GroupName} API",
                                Version = description.ApiVersion.ToString(),
                                Description = $"{description.GroupName} API description."
                            };

                            if (description.IsDeprecated)
                                openApiInfo.Description += " This API version has been deprecated.";

                            options.SwaggerDoc(description.GroupName, openApiInfo);
                        }
                    }
                }

                // Adding swagger data annotation support with [SwaggerOperation] attribute.
                options.EnableAnnotations();
            });


        return services;

        static string XmlCommentsFilePath(Assembly assembly)
        {
            var basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var fileName = assembly.GetName().Name + ".xml";
            return Path.Combine(basePath, fileName);
        }
    }

    /// <summary>
    ///     Register Swagger endpoints.
    ///     Hint: Minimal Api not supported api versioning in .Net6.
    /// </summary>
    public static IApplicationBuilder UseCustomSwagger(
        this IApplicationBuilder app,
        IApiVersionDescriptionProvider provider = null)
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
