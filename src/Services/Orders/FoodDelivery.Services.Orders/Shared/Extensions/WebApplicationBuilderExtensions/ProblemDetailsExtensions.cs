using BuildingBlocks.Web.ProblemDetail;
using Microsoft.AspNetCore.Diagnostics;

namespace FoodDelivery.Services.Orders.Shared.Extensions.WebApplicationBuilderExtensions;

public static partial class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddAppProblemDetails(this WebApplicationBuilder builder)
    {
        builder.Services.AddCustomProblemDetails(problemDetailsOptions =>
        {
            // customization problem details should go here
            problemDetailsOptions.CustomizeProblemDetails = problemDetailContext =>
            {
                // available in .net 8 preview 5
                // https://github.com/dotnet/aspnetcore/pull/47760
                var actualException = problemDetailContext.Exception;

                // with help of capture exception middleware for capturing actual exception
                // https://github.com/dotnet/aspnetcore/issues/4765
                // https://github.com/dotnet/aspnetcore/pull/47760
                // .net 8 will add `IExceptionHandlerFeature`in `DisplayExceptionContent` and `SetExceptionHandlerFeatures` methods `DeveloperExceptionPageMiddlewareImpl` class, exact functionality of CaptureException
                // bet before .net 8 preview 5 we should add `IExceptionHandlerFeature` manually with our `UseCaptureException`
                if (problemDetailContext.HttpContext.Features.Get<IExceptionHandlerFeature>() is { } exceptionFeature)
                { }
            };
        });

        return builder;
    }
}
