using BuildingBlocks.Web.Problem;
using Microsoft.AspNetCore.Diagnostics;

namespace FoodDelivery.Services.Customers.Shared.Extensions.WebApplicationBuilderExtensions;

public static partial class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddAppProblemDetails(this WebApplicationBuilder builder)
    {
        builder.Services.AddCustomProblemDetails(
            problemDetailsOptions =>
            {
                // customization problem details should go here
                problemDetailsOptions.CustomizeProblemDetails = problemDetailContext =>
                {
                    // with help of capture exception middleware for capturing actual exception
                    // https://github.com/dotnet/aspnetcore/issues/4765
                    // https://github.com/dotnet/aspnetcore/pull/47760
                    // `problemDetailContext` doesn't contain real `exception` it will add in this pull request in .net 8
                    if (
                        problemDetailContext.HttpContext.Features.Get<IExceptionHandlerFeature>() is
                        { } exceptionFeature
                    ) { }
                };
            },
            typeof(CustomersMetadata).Assembly
        );

        return builder;
    }
}
