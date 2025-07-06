using BuildingBlocks.Core.Exception;
using BuildingBlocks.Core.Types.Extensions;
using FoodDelivery.Services.Identity.Shared.Models;
using FoodDelivery.Services.Shared.Identity.Users;

namespace FoodDelivery.Services.Identity.Users.Features.UpdatingUserState.v1;

internal class UserStateCannotBeChangedException : AppException
{
    public UserState State { get; }
    public Guid UserId { get; }

    public UserStateCannotBeChangedException(UserState state, Guid userId)
        : base(
            $"User state cannot be changed to: '{state.ToName()}' for user with ID: '{userId}'.",
            StatusCodes.Status500InternalServerError
        )
    {
        State = state;
        UserId = userId;
    }
}
