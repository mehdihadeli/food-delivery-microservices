using Asp.Versioning;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Web.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddCustomVersioning(this IHostApplicationBuilder builder)
    {
        builder
            .Services.AddApiVersioning(options =>
            {
                // reporting api versions will return the headers
                // "api-supported-versions" and "api-deprecated-versions"
                options.ReportApiVersions = true;

                // default `ApiVersionReader` is combine of both `QueryStringApiVersionReader` and `UrlSegmentApiVersionReader`
                // Defines how an API version is read from the current HTTP request
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new HeaderApiVersionReader("api-version"),
                    new QueryStringApiVersionReader(),
                    new UrlSegmentApiVersionReader()
                );

                // AssumeDefaultVersionWhenUnspecified should only be enabled when supporting legacy services that did not previously
                // support API versioning. Forcing existing clients to specify an explicit API version for an
                // existing service introduces a breaking change. Conceptually, clients in this situation are
                // bound to some API version of a service, but they don't know what it is and never explicit request it.
                options.AssumeDefaultVersionWhenUnspecified = true;

                // the default value of `DefaultApiVersion` is  `ApiVersion(1, new int?(0)`
                options.DefaultApiVersion = new ApiVersion(1, 0);

                options
                    .Policies.Sunset(0.9)
                    .Effective(DateTimeOffset.Now.AddDays(60))
                    .Link("policy.html")
                    .Title("Versioning Policy")
                    .Type("text/html");
            })
            .AddApiExplorer(options =>
            {
                // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                // note: the specified format code will format the version as "'v'major[.minor][-status]"
                options.GroupNameFormat = "'v'VVV";

                // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                // can also be used to control the format of the API version in route templates
                options.SubstituteApiVersionInUrl = true;
            })
            .EnableApiVersionBinding(); // this enables binding ApiVersion as a endpoint callback parameter. if you don't use it, then you should remove this configuration.

        return builder;
    }
}
