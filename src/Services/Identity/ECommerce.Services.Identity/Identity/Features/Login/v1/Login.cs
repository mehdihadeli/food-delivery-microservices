using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Exception;
using BuildingBlocks.Core.Exception.Types;
using BuildingBlocks.Core.Utils;
using BuildingBlocks.Security.Jwt;
using ECommerce.Services.Identity.Identity.Exceptions;
using ECommerce.Services.Identity.Identity.Features.GeneratingJwtToken.v1;
using ECommerce.Services.Identity.Identity.Features.GeneratingRefreshToken.v1;
using ECommerce.Services.Identity.Shared.Data;
using ECommerce.Services.Identity.Shared.Exceptions;
using ECommerce.Services.Identity.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ECommerce.Services.Identity.Identity.Features.Login.v1;

public record Login(string UserNameOrEmail, string Password, bool Remember) :
    ICommand<LoginResponse>, ITxRequest;

internal class LoginValidator : AbstractValidator<Login>
{
    public LoginValidator()
    {
        RuleFor(x => x.UserNameOrEmail).NotEmpty().WithMessage("UserNameOrEmail cannot be empty");
        RuleFor(x => x.Password).NotEmpty().WithMessage("password cannot be empty");
    }
}

internal class LoginHandler : ICommandHandler<Login, LoginResponse>
{
    private readonly ICommandProcessor _commandProcessor;
    private readonly IJwtService _jwtService;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<LoginHandler> _logger;
    private readonly IQueryProcessor _queryProcessor;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IdentityContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoginHandler(
        UserManager<ApplicationUser> userManager,
        ICommandProcessor commandProcessor,
        IQueryProcessor queryProcessor,
        IJwtService jwtService,
        IOptions<JwtOptions> jwtOptions,
        SignInManager<ApplicationUser> signInManager,
        IdentityContext context,
        ILogger<LoginHandler> logger)
    {
        _userManager = userManager;
        _commandProcessor = commandProcessor;
        _queryProcessor = queryProcessor;
        _jwtService = jwtService;
        _signInManager = signInManager;
        _context = context;
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
    }

    public async Task<LoginResponse> Handle(Login request, CancellationToken cancellationToken)
    {
        Guard.Against.Null(request, nameof(Login));

        var identityUser = await _userManager.FindByNameAsync(request.UserNameOrEmail) ??
                           await _userManager.FindByEmailAsync(request.UserNameOrEmail);

        Guard.Against.Null(identityUser, new IdentityUserNotFoundException(request.UserNameOrEmail));

        // instead of PasswordSignInAsync, we use CheckPasswordSignInAsync because we don't want set cookie, instead we use JWT
        var signinResult = await _signInManager.CheckPasswordSignInAsync(
            identityUser,
            request.Password,
            false);

        if (signinResult.IsNotAllowed)
        {
            if (!await _userManager.IsEmailConfirmedAsync(identityUser))
                throw new EmailNotConfirmedException(identityUser.Email);

            if (!await _userManager.IsPhoneNumberConfirmedAsync(identityUser))
                throw new PhoneNumberNotConfirmedException(identityUser.PhoneNumber);
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

        var refreshToken =
            (await _commandProcessor.SendAsync(new GenerateRefreshToken(identityUser.Id), cancellationToken))
            .RefreshToken;

        var accessToken =
            await _commandProcessor.SendAsync(
                new GenerateJwtToken(identityUser, refreshToken.Token),
                cancellationToken);

        if (string.IsNullOrWhiteSpace(accessToken.Token))
            throw new AppException("Generate access token failed.");

        _logger.LogInformation("User with ID: {ID} has been authenticated", identityUser.Id);

        if (_jwtOptions.CheckRevokedAccessTokens)
        {
            await _context.Set<AccessToken>().AddAsync(
                new AccessToken
                {
                    UserId = identityUser.Id,
                    Token = accessToken.Token,
                    CreatedAt = DateTime.Now,
                    ExpiredAt = accessToken.ExpireAt,
                    CreatedByIp = IpUtilities.GetIpAddress()
                },
                cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
        }

        // we can don't return value from command and get token from a short term session in our request with `TokenStorageService`
        return new LoginResponse(identityUser, accessToken.Token, refreshToken.Token);
    }
}
