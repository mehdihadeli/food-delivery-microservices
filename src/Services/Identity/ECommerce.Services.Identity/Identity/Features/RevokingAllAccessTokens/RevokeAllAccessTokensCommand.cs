using BuildingBlocks.Abstractions.Caching;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Security.Jwt;
using ECommerce.Services.Identity.Identity.Features.RevokingAccessToken;
using ECommerce.Services.Identity.Shared.Exceptions;
using ECommerce.Services.Identity.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ECommerce.Services.Identity.Identity.Features.RevokingAllAccessTokens;

public record RevokeAllAccessTokensCommand(string UserName) : ICommand;

public class RevokeAllAccessTokenCommandHandler : ICommandHandler<RevokeAllAccessTokensCommand>
{
    private readonly IMediator _mediator;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IdentityDbContext _identityDbContext;

    public RevokeAllAccessTokenCommandHandler(
        IdentityDbContext identityDbContext,
        IMediator mediator,
        UserManager<ApplicationUser> userManager)
    {
        _identityDbContext = identityDbContext;
        _mediator = mediator;
        _userManager = userManager;
    }

    public async Task<Unit> Handle(RevokeAllAccessTokensCommand request, CancellationToken cancellationToken)
    {
        var appUser = await _userManager.FindByNameAsync(request.UserName);
        if (appUser == null)
        {
            throw new UserNotFoundException(request.UserName);
        }

        var tokens = _identityDbContext.Set<AccessToken>()
            .Where(x => x.UserId == appUser.Id && x.ExpiredAt > DateTime.Now);

        foreach (var accessToken in tokens)
        {
            await _mediator.Send(new RevokeAccessTokenCommand(accessToken.Token, appUser.UserName), cancellationToken);
        }

        return Unit.Value;
    }
}
