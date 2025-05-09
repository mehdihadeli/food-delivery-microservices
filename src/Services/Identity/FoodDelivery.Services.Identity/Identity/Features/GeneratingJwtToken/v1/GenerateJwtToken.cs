using System.Collections.Immutable;
using System.Security.Claims;
using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Security;
using BuildingBlocks.Security.Jwt;
using FoodDelivery.Services.Identity.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace FoodDelivery.Services.Identity.Identity.Features.GeneratingJwtToken.v1;

public record GenerateJwtToken(ApplicationUser User, string RefreshToken) : ICommand<GenerateJwtTokenResult>
{
    /// <summary>
    /// GenerateJwtToken with in-line validation.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="refreshToken"></param>
    /// <returns></returns>
    public static GenerateJwtToken Of(ApplicationUser? user, string? refreshToken)
    {
        user.NotBeNull();
        refreshToken.NotBeNull();

        return new GenerateJwtToken(user, refreshToken);
    }
}

public class GenerateJwtTokenHandler(
    UserManager<ApplicationUser> userManager,
    IJwtService jwtService,
    ILogger<GenerateJwtTokenHandler> logger
) : ICommandHandler<GenerateJwtToken, GenerateJwtTokenResult>
{
    public async ValueTask<GenerateJwtTokenResult> Handle(GenerateJwtToken request, CancellationToken cancellationToken)
    {
        request.NotBeNull();

        var identityUser = request.User;
        identityUser.NotBeNull();

        // authentication successful so generate jwt and refresh tokens
        var allClaims = await GetClaimsAsync(request.User.UserName!);
        var fullName = $"{identityUser.FirstName} {identityUser.LastName}";

        var tokenResult = jwtService.GenerateJwtToken(
            identityUser.UserName!,
            identityUser.Email!,
            identityUser.Id.ToString(),
            identityUser.EmailConfirmed || identityUser.PhoneNumberConfirmed,
            fullName,
            request.RefreshToken,
            allClaims.UserClaims.ToImmutableList(),
            allClaims.Roles.ToImmutableList(),
            allClaims.PermissionClaims.ToImmutableList()
        );

        logger.LogInformation("access-token generated, \n: {AccessToken}", tokenResult.AccessToken);

        return new GenerateJwtTokenResult(tokenResult.AccessToken, tokenResult.ExpireAt);
    }

    private async Task<(IList<Claim> UserClaims, IList<string> Roles, IList<string> PermissionClaims)> GetClaimsAsync(
        string userName
    )
    {
        var appUser = await userManager.FindByNameAsync(userName);
        appUser.NotBeNull();

        var userClaims = (await userManager.GetClaimsAsync(appUser))
            .Where(x => x.Type != ClaimsType.Permission)
            .ToList();
        var roles = await userManager.GetRolesAsync(appUser);

        var permissions = (await userManager.GetClaimsAsync(appUser))
            .Where(x => x.Type == ClaimsType.Permission)
            ?.Select(x => x.Value)
            .ToList();

        return (UserClaims: userClaims, Roles: roles, PermissionClaims: permissions);
    }
}

public record GenerateJwtTokenResult(string Token, DateTime ExpireAt);
