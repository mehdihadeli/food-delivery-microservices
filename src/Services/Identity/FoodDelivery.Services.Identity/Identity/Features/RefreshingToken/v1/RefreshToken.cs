using System.Security.Claims;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Security.Jwt;
using BuildingBlocks.Validation.Extensions;
using FoodDelivery.Services.Identity.Identity.Exceptions;
using FoodDelivery.Services.Identity.Identity.Features.GeneratingJwtToken.v1;
using FoodDelivery.Services.Identity.Identity.Features.GeneratingRefreshToken.v1;
using FoodDelivery.Services.Identity.Shared.Exceptions;
using FoodDelivery.Services.Identity.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace FoodDelivery.Services.Identity.Identity.Features.RefreshingToken.v1;

internal record RefreshToken(string AccessTokenData, string RefreshTokenData) : ICommand<RefreshTokenResult>
{
    /// <summary>
    /// RefreshToken with in-line validation.
    /// </summary>
    /// <param name="accessTokenData"></param>
    /// <param name="refreshTokenData"></param>
    /// <returns></returns>
    public static RefreshToken Of(string? accessTokenData, string? refreshTokenData)
    {
        return new RefreshTokenValidator().HandleValidation(new RefreshToken(accessTokenData!, refreshTokenData!));
    }
}

internal class RefreshTokenValidator : AbstractValidator<RefreshToken>
{
    public RefreshTokenValidator()
    {
        RuleFor(v => v.AccessTokenData).NotEmpty();
        RuleFor(v => v.RefreshTokenData).NotEmpty();
    }
}

internal class RefreshTokenHandler : ICommandHandler<RefreshToken, RefreshTokenResult>
{
    private readonly ICommandProcessor _commandProcessor;
    private readonly IJwtService _jwtService;
    private readonly UserManager<ApplicationUser> _userManager;

    public RefreshTokenHandler(
        IJwtService jwtService,
        UserManager<ApplicationUser> userManager,
        ICommandProcessor commandProcessor
    )
    {
        _jwtService = jwtService;
        _userManager = userManager;
        _commandProcessor = commandProcessor;
    }

    public async Task<RefreshTokenResult> Handle(RefreshToken command, CancellationToken cancellationToken)
    {
        command.NotBeNull();

        // invalid token/signing key was passed and we can't extract user claims
        var userClaimsPrincipal = _jwtService.GetPrincipalFromToken(command.AccessTokenData);

        if (userClaimsPrincipal is null)
            throw new InvalidTokenException(userClaimsPrincipal);

        var userId = userClaimsPrincipal.FindFirstValue(JwtRegisteredClaimNames.NameId);

        var identityUser = await _userManager.FindByIdAsync(userId);

        if (identityUser == null)
            throw new IdentityUserNotFoundException(userId);

        var refreshToken = (
            await _commandProcessor.SendAsync(
                GenerateRefreshToken.Of(identityUser.Id, command.RefreshTokenData),
                cancellationToken
            )
        ).RefreshToken;

        var accessToken = await _commandProcessor.SendAsync(
            GenerateJwtToken.Of(identityUser, refreshToken.Token),
            cancellationToken
        );

        return new RefreshTokenResult(
            identityUser.Id,
            identityUser.UserName!,
            identityUser.FirstName,
            identityUser.LastName,
            accessToken.Token,
            refreshToken.Token
        );
    }
}

internal record RefreshTokenResult(
    Guid UserId,
    string UserName,
    string FirstName,
    string LastName,
    string AccessToken,
    string RefreshToken
);
