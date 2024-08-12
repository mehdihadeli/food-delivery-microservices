using BuildingBlocks.Core.Exception.Types;
using BuildingBlocks.Core.Types.Extensions;
using FoodDelivery.Services.Identity.Shared.Models;

namespace FoodDelivery.Services.Identity.Users.Features.UpdatingUserState.V1;

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
