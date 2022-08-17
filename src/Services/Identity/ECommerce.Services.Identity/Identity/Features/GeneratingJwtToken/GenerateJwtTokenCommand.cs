using System.Collections.Immutable;
using System.Security.Claims;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Security.Jwt;
using ECommerce.Services.Identity.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Services.Identity.Identity.Features.GeneratingJwtToken;

public record GenerateJwtTokenCommand
    (ApplicationUser User, string RefreshToken) : ICommand<GenerateJwtTokenCommandResult>;

public class GenerateJwtTokenCommandHandler : ICommandHandler<GenerateJwtTokenCommand, GenerateJwtTokenCommandResult>
{
    private readonly ILogger<GenerateJwtTokenCommandHandler> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;

    public GenerateJwtTokenCommandHandler(
        UserManager<ApplicationUser> userManager,
        IJwtService jwtService,
        ILogger<GenerateJwtTokenCommandHandler> logger)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<GenerateJwtTokenCommandResult> Handle(
        GenerateJwtTokenCommand request,
        CancellationToken cancellationToken)
    {
        var identityUser = request.User;

        // authentication successful so generate jwt and refresh tokens
        var allClaims = await GetClaimsAsync(request.User.UserName);
        var fullName = $"{identityUser.FirstName} {identityUser.LastName}";

        var tokenResult = _jwtService.GenerateJwtToken(
            identityUser.UserName,
            identityUser.Email,
            identityUser.Id.ToString(),
            identityUser.EmailConfirmed || identityUser.PhoneNumberConfirmed,
            fullName,
            request.RefreshToken,
            allClaims.UserClaims.ToImmutableList(),
            allClaims.Roles.ToImmutableList(),
            allClaims.PermissionClaims.ToImmutableList());

        _logger.LogInformation("access-token generated, \n: {AccessToken}", tokenResult.AccessToken);

        return new GenerateJwtTokenCommandResult(tokenResult.AccessToken, tokenResult.ExpireAt);
    }

    public async Task<(IList<Claim> UserClaims, IList<string> Roles, IList<string> PermissionClaims)>
        GetClaimsAsync(string userName)
    {
        var appUser = await _userManager.FindByNameAsync(userName);
        var userClaims =
            (await _userManager.GetClaimsAsync(appUser)).Where(x => x.Type != CustomClaimTypes.Permission).ToList();
        var roles = await _userManager.GetRolesAsync(appUser);

        var permissions = (await _userManager.GetClaimsAsync(appUser))
            .Where(x => x.Type == CustomClaimTypes.Permission)?.Select(x => x
                .Value).ToList();

        return (UserClaims: userClaims, Roles: roles, PermissionClaims: permissions);
    }
}

public record GenerateJwtTokenCommandResult(string Token, DateTime ExpireAt);
