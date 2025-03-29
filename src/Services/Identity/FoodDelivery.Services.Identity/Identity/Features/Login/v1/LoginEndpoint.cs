using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Web.ProblemDetail.HttpResults;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Identity.Identity.Features.Login.v1;

internal static class LoginEndpoint
{
    internal static RouteHandlerBuilder MapLoginUserEndpoint(this IEndpointRouteBuilder endpoints)
    {
        // https://github.com/dotnet/aspnetcore/issues/45082
        // https://github.com/dotnet/aspnetcore/issues/40753
        // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/pull/2414
        return endpoints
            .MapPost("/login", Handle)
            .AllowAnonymous()
            .MapToApiVersion(1.0)
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?#typedresults-vs-results
            // .Produces<LoginResponse>(StatusCodes.Status200OK)
            // .ProducesProblem(StatusCodes.Status404NotFound)
            // .ProducesProblem(StatusCodes.Status500InternalServerError)
            // .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .WithName(nameof(Login))
            .WithDisplayName(nameof(Login).Humanize())
            .WithSummary(nameof(Login).Humanize())
            .WithDescription(nameof(Login).Humanize());

        async Task<
            Results<Ok<LoginResponse>, InternalHttpProblemResult, ForbiddenHttpProblemResult, ValidationProblem>
        > Handle([AsParameters] LoginRequestParameters requestParameters)
        {
            var (request, context, commandBus, cancellationToken) = requestParameters;

            var command = Login.Of(request.UserNameOrEmail, request.Password, request.Remember);

            var result = await commandBus.SendAsync(command, cancellationToken);

            var response = result.ToLoginResponse();

            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-7.0#multiple-response-types
            return TypedResults.Ok(response);
        }
    }
}

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#parameter-binding-for-argument-lists-with-asparameters
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#binding-precedence
internal record LoginRequestParameters(
    [FromBody] LoginRequest Request,
    HttpContext HttpContext,
    ICommandBus CommandBus,
    CancellationToken CancellationToken
) : IHttpCommand<LoginRequest>;

// These parameters can be pass null from the user
internal record LoginRequest(string? UserNameOrEmail, string? Password, bool Remember);

internal record LoginResponse(
    Guid UserId,
    string UserName,
    string FirstName,
    string LastName,
    string AccessToken,
    string RefreshToken
);
