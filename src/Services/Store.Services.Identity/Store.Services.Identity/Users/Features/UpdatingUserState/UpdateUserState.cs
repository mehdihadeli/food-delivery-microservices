using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Core.Exception;
using Store.Services.Identity.Shared.Exceptions;
using Store.Services.Identity.Shared.Models;
using Store.Services.Shared.Identity.Users.Events.Integration;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using UserState = Store.Services.Identity.Shared.Models.UserState;

namespace Store.Services.Identity.Users.Features.UpdatingUserState;

public record UpdateUserState(Guid UserId, UserState State) : ITxUpdateCommand;

internal class UpdateUserStateValidator : AbstractValidator<UpdateUserState>
{
    public UpdateUserStateValidator()
    {
        CascadeMode = CascadeMode.Stop;

        RuleFor(v => v.State)
            .NotEmpty();

        RuleFor(v => v.UserId)
            .NotEmpty();
    }
}

internal class UpdateUserStateHandler : ICommandHandler<UpdateUserState>
{
    private readonly IBus _bus;
    private readonly ILogger<UpdateUserStateHandler> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public UpdateUserStateHandler(
        IBus bus,
        UserManager<ApplicationUser> userManager,
        ILogger<UpdateUserStateHandler> logger)
    {
        _bus = bus;
        _logger = logger;
        _userManager = Guard.Against.Null(userManager, nameof(userManager));
    }

    public async Task<Unit> Handle(UpdateUserState request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        Guard.Against.Null(user, new UserNotFoundException(request.UserId));

        var previousState = user.UserState;
        if (previousState == request.State)
        {
            return Unit.Value;
        }

        if (await _userManager.IsInRoleAsync(user, Constants.Role.Admin))
        {
            throw new UserStateCannotBeChangedException(request.State, request.UserId);
        }

        user.UserState = request.State;

        await _userManager.UpdateAsync(user);

        var userStateUpdated = new UserStateUpdated(
            request.UserId,
            (Services.Shared.Identity.Users.Events.Integration.UserState)(int)previousState,
            (Services.Shared.Identity.Users.Events.Integration.UserState)(int)request.State);

        await _bus.PublishAsync(userStateUpdated, null, cancellationToken);


        _logger.LogInformation(
            "Updated state for user with ID: '{UserId}', '{PreviousState}' -> '{UserState}'",
            user.Id,
            previousState,
            user.UserState);

        return Unit.Value;
    }
}
