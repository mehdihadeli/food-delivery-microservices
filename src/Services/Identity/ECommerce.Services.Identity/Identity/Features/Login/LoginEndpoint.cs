using BuildingBlocks.Abstractions.CQRS.Command;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Services.Identity.Identity.Features.Login;

public static class LoginEndpoint
{
    internal static IEndpointRouteBuilder MapLoginUserEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"{IdentityConfigs.IdentityPrefixUri}/login", LoginUser)
            .AllowAnonymous()
            .WithTags(IdentityConfigs.Tag)
            .Produces<LoginResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .WithDisplayName("Login User.");

        return endpoints;
    }

    private static async Task<IResult> LoginUser(
        LoginUserRequest request,
        ICommandProcessor commandProcessor,
        CancellationToken cancellationToken)
    {
        var command = new Login(request.UserNameOrEmail, request.Password, request.Remember);

        var result = await commandProcessor.SendAsync(command, cancellationToken);

        return Results.Ok(result);
    }
}
