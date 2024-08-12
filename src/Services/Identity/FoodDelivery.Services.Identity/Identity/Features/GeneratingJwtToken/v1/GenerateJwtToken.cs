using System.Collections.Immutable;
using System.Security.Claims;
using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Security.Jwt;
using FoodDelivery.Services.Identity.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace FoodDelivery.Services.Identity.Identity.Features.GeneratingJwtToken.V1;

internal record GenerateJwtToken(ApplicationUser User, string RefreshToken) : ICommand<GenerateJwtTokenResult>
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

internal class GenerateJwtTokenHandler : ICommandHandler<GenerateJwtToken, GenerateJwtTokenResult>
{
    private readonly ILogger<GenerateJwtTokenHandler> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;

    public GenerateJwtTokenHandler(
        UserManager<ApplicationUser> userManager,
        IJwtService jwtService,
        ILogger<GenerateJwtTokenHandler> logger
    )
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<GenerateJwtTokenResult> Handle(GenerateJwtToken request, CancellationToken cancellationToken)
    {
        request.NotBeNull();

        var identityUser = request.User;
        identityUser.NotBeNull();

        // authentication successful so generate jwt and refresh tokens
        var allClaims = await GetClaimsAsync(request.User.UserName!);
        var fullName = $"{identityUser.FirstName} {identityUser.LastName}";

        var tokenResult = _jwtService.GenerateJwtToken(
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

        _logger.LogInformation("access-token generated, \n: {AccessToken}", tokenResult.AccessToken);

        return new GenerateJwtTokenResult(tokenResult.AccessToken, tokenResult.ExpireAt);
    }

    private async Task<(IList<Claim> UserClaims, IList<string> Roles, IList<string> PermissionClaims)> GetClaimsAsync(
        string userName
    )
    {
        var appUser = await _userManager.FindByNameAsync(userName);
        appUser.NotBeNull();

        var userClaims = (await _userManager.GetClaimsAsync(appUser))
            .Where(x => x.Type != CustomClaimTypes.Permission)
            .ToList();
        var roles = await _userManager.GetRolesAsync(appUser);

        var permissions = (await _userManager.GetClaimsAsync(appUser))
            .Where(x => x.Type == CustomClaimTypes.Permission)
            ?.Select(x => x.Value)
            .ToList();

        return (UserClaims: userClaims, Roles: roles, PermissionClaims: permissions);
    }
}

public record GenerateJwtTokenResult(string Token, DateTime ExpireAt);
