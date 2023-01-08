using System.Globalization;
using Microsoft.AspNetCore.Builder;

namespace BuildingBlocks.Web.Extensions;

public static class EndpointConventionBuilderExtensions
{
    public static RouteHandlerBuilder WithResponseDescription(
        this RouteHandlerBuilder builder,
        int statusCode,
        string description)
    {
        builder.WithOpenApi(operation =>
        {
            operation.Responses[statusCode.ToString(CultureInfo.InvariantCulture)].Description = description;
            return operation;
        });
        return builder;
    }
}
