using BuildingBlocks.Abstractions.CQRS.Commands;
using Hellang.Middleware.ProblemDetails;

namespace ECommerce.Services.Identity.Identity.Features.Login.v1;

public static class LoginEndpoint
{
    internal static RouteHandlerBuilder MapLoginUserEndpoint(this IEndpointRouteBuilder endpoints)
    {
        // https://github.com/dotnet/aspnetcore/issues/45082
        // https://github.com/dotnet/aspnetcore/issues/40753
        // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/pull/2414
        return endpoints.MapPost("/login", LoginUser)
            .AllowAnonymous()
            .Produces<LoginResponse>()
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status500InternalServerError)
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status400BadRequest)
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Login User", Description = "Login User"
            })
            .WithDisplayName("Login User.")
            .WithName("Login")
            .MapToApiVersion(1.0);
    }

    private static async Task<IResult> LoginUser(
        LoginRequest request,
        ICommandProcessor commandProcessor,
        CancellationToken cancellationToken)
    {
        var command = new Login(request.UserNameOrEmail, request.Password, request.Remember);

        var result = await commandProcessor.SendAsync(command, cancellationToken);

        return Results.Ok(result);
    }
}
