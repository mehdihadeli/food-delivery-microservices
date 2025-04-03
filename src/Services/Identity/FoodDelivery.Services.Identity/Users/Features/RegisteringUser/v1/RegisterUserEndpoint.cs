using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using FoodDelivery.Services.Identity.Users.Dtos.v1;
using FoodDelivery.Services.Identity.Users.Features.GettingUserById.v1;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Identity.Users.Features.RegisteringUser.v1;

public static class RegisterUserEndpoint
{
    internal static RouteHandlerBuilder MapRegisterNewUserEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapPost("/", Handle)
            .AllowAnonymous()
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?#typedresults-vs-results
            // .Produces<RegisterUserResponse>(StatusCodes.Status201Created)
            // .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .WithName(nameof(RegisterUser))
            .WithSummary(nameof(RegisterUser).Humanize())
            .WithDescription(nameof(RegisterUser).Humanize())
            .WithDisplayName(nameof(RegisterUser).Humanize())
            .MapToApiVersion(1.0);

        async Task<Results<CreatedAtRoute<RegisterUserResponse>, ValidationProblem>> Handle(
            [AsParameters] RegisterUserRequestParameters requestParameters
        )
        {
            var (request, context, commandBus, cancellationToken) = requestParameters;

            var command = request.ToRegisterUser();

            var result = await commandBus.SendAsync(command, cancellationToken);

            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-7.0#multiple-response-types
            return TypedResults.CreatedAtRoute(
                new RegisterUserResponse(result.UserIdentity),
                nameof(GetUserById),
                new { id = result.UserIdentity?.Id }
            );
        }
    }
}

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#parameter-binding-for-argument-lists-with-asparameters
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#binding-precedence
internal record RegisterUserRequestParameters(
    [FromBody] RegisterUserRequest Request,
    HttpContext HttpContext,
    ICommandBus CommandBus,
    CancellationToken CancellationToken
) : IHttpCommand<RegisterUserRequest>;

// parameters can be pass as null from the user
internal record RegisterUserRequest(
    string? FirstName,
    string? LastName,
    string? UserName,
    string? Email,
    string? PhoneNumber,
    string? Password,
    string? ConfirmPassword
)
{
    public List<string> Roles { get; init; } = [IdentityConstants.Role.User];
}

internal record RegisterUserResponse(IdentityUserDto? UserIdentity);
