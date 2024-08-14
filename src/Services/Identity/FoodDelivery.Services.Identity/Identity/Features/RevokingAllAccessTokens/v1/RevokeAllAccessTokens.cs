using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Identity.Identity.Features.RevokingAccessToken.v1;
using FoodDelivery.Services.Identity.Shared.Exceptions;
using FoodDelivery.Services.Identity.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace FoodDelivery.Services.Identity.Identity.Features.RevokingAllAccessTokens.v1;

internal record RevokeAllAccessTokens(string UserName) : ICommand
{
    /// <summary>
    /// RevokeAllAccessTokens with in-line validation.
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public static RevokeAllAccessTokens Of(string? userName) => new(userName.NotBeEmptyOrNull());
}

internal class RevokeAllAccessTokenHandler : ICommandHandler<RevokeAllAccessTokens>
{
    private readonly IMediator _mediator;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IdentityDbContext _identityDbContext;

    public RevokeAllAccessTokenHandler(
        IdentityDbContext identityDbContext,
        IMediator mediator,
        UserManager<ApplicationUser> userManager
    )
    {
        _identityDbContext = identityDbContext;
        _mediator = mediator;
        _userManager = userManager;
    }

    public async Task Handle(RevokeAllAccessTokens request, CancellationToken cancellationToken)
    {
        var appUser = await _userManager.FindByNameAsync(request.UserName);
        appUser.NotBeNull(new IdentityUserNotFoundException(request.UserName));

        var tokens = _identityDbContext
            .Set<AccessToken>()
            .Where(x => x.UserId == appUser.Id && x.ExpiredAt > DateTime.Now);

        foreach (var accessToken in tokens)
        {
            await _mediator.Send(new RevokeAccessToken(accessToken.Token, appUser.UserName!), cancellationToken);
        }
    }
}
