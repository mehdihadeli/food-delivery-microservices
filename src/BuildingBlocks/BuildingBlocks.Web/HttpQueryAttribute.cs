using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc.Routing;

namespace BuildingBlocks.Web;

public class HttpQueryAttribute : HttpMethodAttribute
{
    private static readonly IEnumerable<string> _supportedMethods = new[] {"QUERY"};

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpQueryAttribute"/> class.
    /// Creates a new <see cref="Microsoft.AspNetCore.Mvc.HttpGetAttribute"/>.
    /// </summary>
    public HttpQueryAttribute()
        : base(_supportedMethods)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpQueryAttribute"/> class.
    /// Creates a new <see cref="Microsoft.AspNetCore.Mvc.HttpGetAttribute"/> with the given route template.
    /// </summary>
    /// <param name="template">The route template. May not be null.</param>
    public HttpQueryAttribute([StringSyntax("Route")] string template)
        : base(_supportedMethods, template)
    {
        if (template == null)
        {
            throw new ArgumentNullException(nameof(template));
        }
    }
}
