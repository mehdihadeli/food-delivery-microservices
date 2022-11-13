using Asp.Versioning.Conventions;
using BuildingBlocks.Abstractions.CQRS.Commands;
using ECommerce.Services.Identity.Shared;

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
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status400BadRequest)
            .WithDisplayName("Login User.")
            .WithApiVersionSet(SharedModulesConfiguration.VersionSet)
            .HasApiVersion(1.0);

        return endpoints;
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
