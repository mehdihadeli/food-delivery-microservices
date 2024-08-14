using System.Security.Claims;
using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Security.Jwt;
using BuildingBlocks.Validation.Extensions;
using FluentValidation;
using FoodDelivery.Services.Identity.Identity.Exceptions;
using FoodDelivery.Services.Identity.Identity.Features.GeneratingJwtToken.v1;
using FoodDelivery.Services.Identity.Identity.Features.GeneratingRefreshToken.v1;
using FoodDelivery.Services.Identity.Shared.Exceptions;
using FoodDelivery.Services.Identity.Shared.Models;
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

internal class RefreshTokenHandler(
    IJwtService jwtService,
    UserManager<ApplicationUser> userManager,
    ICommandBus commandBus
) : ICommandHandler<RefreshToken, RefreshTokenResult>
{
    public async Task<RefreshTokenResult> Handle(RefreshToken command, CancellationToken cancellationToken)
    {
        command.NotBeNull();

        // invalid token/signing key was passed and we can't extract user claims
        var userClaimsPrincipal = jwtService.GetPrincipalFromToken(command.AccessTokenData);

        if (userClaimsPrincipal is null)
            throw new InvalidTokenException(userClaimsPrincipal);

        var userId = userClaimsPrincipal.FindFirstValue(JwtRegisteredClaimNames.NameId);

        var identityUser = await userManager.FindByIdAsync(userId);

        if (identityUser == null)
            throw new IdentityUserNotFoundException(userId);

        var refreshToken = (
            await commandBus.SendAsync(
                GenerateRefreshToken.Of(identityUser.Id, command.RefreshTokenData),
                cancellationToken
            )
        ).RefreshToken;

        var accessToken = await commandBus.SendAsync(
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
