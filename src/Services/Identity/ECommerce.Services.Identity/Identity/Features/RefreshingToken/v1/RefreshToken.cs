using System.Security.Claims;
using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Security.Jwt;
using ECommerce.Services.Identity.Identity.Exceptions;
using ECommerce.Services.Identity.Identity.Features.GeneratingJwtToken.v1;
using ECommerce.Services.Identity.Identity.Features.GeneratingRefreshToken.v1;
using ECommerce.Services.Identity.Shared.Exceptions;
using ECommerce.Services.Identity.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace ECommerce.Services.Identity.Identity.Features.RefreshingToken.v1;

public record RefreshToken(string AccessTokenData, string RefreshTokenData) : ICommand<RefreshTokenResponse>;

internal class RefreshTokenValidator : AbstractValidator<RefreshToken>
{
    public RefreshTokenValidator()
    {
        RuleFor(v => v.AccessTokenData)
            .NotEmpty();

        RuleFor(v => v.RefreshTokenData)
            .NotEmpty();
    }
}

internal class RefreshTokenHandler : ICommandHandler<RefreshToken, RefreshTokenResponse>
{
    private readonly ICommandProcessor _commandProcessor;
    private readonly IJwtService _jwtService;
    private readonly UserManager<ApplicationUser> _userManager;

    public RefreshTokenHandler(
        IJwtService jwtService,
        UserManager<ApplicationUser> userManager,
        ICommandProcessor commandProcessor)
    {
        _jwtService = jwtService;
        _userManager = userManager;
        _commandProcessor = commandProcessor;
    }

    public async Task<RefreshTokenResponse> Handle(RefreshToken request, CancellationToken cancellationToken)
    {
        Guard.Against.Null(request, nameof(RefreshToken));

        // invalid token/signing key was passed and we can't extract user claims
        var userClaimsPrincipal = _jwtService.GetPrincipalFromToken(request.AccessTokenData);

        if (userClaimsPrincipal is null)
            throw new InvalidTokenException(userClaimsPrincipal);

        var userId = userClaimsPrincipal.FindFirstValue(JwtRegisteredClaimNames.NameId);

        var identityUser = await _userManager.FindByIdAsync(userId);

        if (identityUser == null)
            throw new IdentityUserNotFoundException(userId);

        var refreshToken =
            (await _commandProcessor.SendAsync(
                new GenerateRefreshToken(identityUser.Id, request.RefreshTokenData),
                cancellationToken)).RefreshToken;

        var accessToken =
            await _commandProcessor.SendAsync(
                new GenerateJwtToken(identityUser, refreshToken.Token), cancellationToken);

        return new RefreshTokenResponse(identityUser, accessToken.Token, refreshToken.Token);
    }
}
