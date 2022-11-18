using ECommerce.Services.Identity.Shared.Models;

namespace ECommerce.Services.Identity.Users.Features.UpdatingUserState.v1;

public record UpdateUserStateRequest
{
    public UserState UserState { get; init; }
}
