using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Abstractions.Queries;
using BuildingBlocks.Core.Exception.Types;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Utils;
using BuildingBlocks.Security.Jwt;
using BuildingBlocks.Validation.Extensions;
using FluentValidation;
using FoodDelivery.Services.Identity.Identity.Exceptions;
using FoodDelivery.Services.Identity.Identity.Features.GeneratingJwtToken.v1;
using FoodDelivery.Services.Identity.Identity.Features.GeneratingRefreshToken.v1;
using FoodDelivery.Services.Identity.Shared.Data;
using FoodDelivery.Services.Identity.Shared.Exceptions;
using FoodDelivery.Services.Identity.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace FoodDelivery.Services.Identity.Identity.Features.Login.V1;

internal record Login(string UserNameOrEmail, string Password, bool Remember) : ICommand<LoginResult>, ITxRequest
{
    /// <summary>
    /// Login with in-line validator.
    /// </summary>
    /// <param name="userNameOrEmail"></param>
    /// <param name="password"></param>
    /// <param name="rememberMe"></param>
    /// <returns></returns>
    public static Login Of(string? userNameOrEmail, string? password, bool rememberMe)
    {
        return new LoginValidator().HandleValidation(new Login(userNameOrEmail!, password!, rememberMe));
    }
}

internal class LoginValidator : AbstractValidator<Login>
{
    public LoginValidator()
    {
        RuleFor(x => x.UserNameOrEmail).NotEmpty().WithMessage("UserNameOrEmail cannot be empty");
        RuleFor(x => x.Password).NotEmpty().WithMessage("password cannot be empty");
    }
}

internal class LoginHandler : ICommandHandler<Login, LoginResult>
{
    private readonly ICommandBus _commandProcessor;
    private readonly IJwtService _jwtService;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<LoginHandler> _logger;
    private readonly IQueryBus _queryBus;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IdentityContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoginHandler(
        UserManager<ApplicationUser> userManager,
        ICommandBus commandProcessor,
        IQueryBus queryBus,
        IJwtService jwtService,
        IOptions<JwtOptions> jwtOptions,
        SignInManager<ApplicationUser> signInManager,
        IdentityContext context,
        ILogger<LoginHandler> logger
    )
    {
        _userManager = userManager;
        _commandProcessor = commandProcessor;
        _queryBus = queryBus;
        _jwtService = jwtService;
        _signInManager = signInManager;
        _context = context;
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
    }

    public async Task<LoginResult> Handle(Login command, CancellationToken cancellationToken)
    {
        command.NotBeNull();
        var identityUser =
            await _userManager.FindByNameAsync(command.UserNameOrEmail)
            ?? await _userManager.FindByEmailAsync(command.UserNameOrEmail);

        identityUser.NotBeNull(exception: new IdentityUserNotFoundException(command.UserNameOrEmail));

        // instead of PasswordSignInAsync, we use CheckPasswordSignInAsync because we don't want set cookie, instead we use JWT
        var signinResult = await _signInManager.CheckPasswordSignInAsync(identityUser, command.Password, false);

        if (signinResult.IsNotAllowed)
        {
            if (!await _userManager.IsEmailConfirmedAsync(identityUser))
                throw new EmailNotConfirmedException(identityUser.Email!);

            if (!await _userManager.IsPhoneNumberConfirmedAsync(identityUser))
                throw new PhoneNumberNotConfirmedException(identityUser.PhoneNumber!);
        }
        else if (signinResult.IsLockedOut)
        {
            throw new UserLockedException(identityUser.Id.ToString());
        }
        else if (signinResult.RequiresTwoFactor)
        {
            throw new RequiresTwoFactorException("Require two factor authentication.");
        }
        else if (!signinResult.Succeeded)
        {
            throw new PasswordIsInvalidException("Password is invalid.");
        }

        var refreshToken = (
            await _commandProcessor.SendAsync(GenerateRefreshToken.Of(identityUser.Id), cancellationToken)
        ).RefreshToken;

        var accessToken = await _commandProcessor.SendAsync(
            GenerateJwtToken.Of(identityUser, refreshToken.Token),
            cancellationToken
        );

        if (string.IsNullOrWhiteSpace(accessToken.Token))
            throw new AppException("Generate access token failed.");

        _logger.LogInformation("User with ID: {ID} has been authenticated", identityUser.Id);

        if (_jwtOptions.CheckRevokedAccessTokens)
        {
            await _context
                .Set<AccessToken>()
                .AddAsync(
                    new AccessToken
                    {
                        UserId = identityUser.Id,
                        Token = accessToken.Token,
                        CreatedAt = DateTime.Now,
                        ExpiredAt = accessToken.ExpireAt,
                        CreatedByIp = IpUtilities.GetIpAddress()
                    },
                    cancellationToken
                );

            await _context.SaveChangesAsync(cancellationToken);
        }

        // we can don't return value from command and get token from a short term session in our request with `TokenStorageService`
        return new LoginResult(
            identityUser.Id,
            identityUser.UserName!,
            identityUser.FirstName,
            identityUser.LastName,
            accessToken.Token,
            refreshToken.Token
        );
    }
}

internal record LoginResult(
    Guid UserId,
    string UserName,
    string FirstName,
    string LastName,
    string AccessToken,
    string RefreshToken
);
