using System.Text;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Primitives;

namespace BuildingBlocks.OpenApi.AspnetOpenApi.Extensions;

public static class ApiVersionDescriptionExtensions
{
    public static string BuildDescription(this ApiVersionDescription api, string? description)
    {
        var text = new StringBuilder(description ?? string.Empty);

        if (api.IsDeprecated)
        {
            text.Append(" This API version has been deprecated.");
        }

        if (api.SunsetPolicy is { } policy)
        {
            if (policy.Date is { } when)
            {
                text.Append(" The API will be sunset on ").Append(when.Date.ToShortDateString()).Append('.');
            }

            if (policy.HasLinks)
            {
                text.AppendLine();

                var rendered = false;

                foreach (var link in policy.Links)
                {
                    if (link.Type == "text/html")
                    {
                        if (!rendered)
                        {
                            text.Append("<h4>Links</h4><ul>");
                            rendered = true;
                        }

                        text.Append("<li><a href=\"");
                        text.Append(link.LinkTarget.OriginalString);
                        text.Append("\">");
                        text.Append(
                            StringSegment.IsNullOrEmpty(link.Title)
                                ? link.LinkTarget.OriginalString
                                : link.Title.ToString()
                        );
                        text.Append("</a></li>");
                    }
                }

                if (rendered)
                {
                    text.Append("</ul>");
                }
            }
        }

        text.Append("<h4>Additional Information</h4>");

        return text.ToString();
    }
}
