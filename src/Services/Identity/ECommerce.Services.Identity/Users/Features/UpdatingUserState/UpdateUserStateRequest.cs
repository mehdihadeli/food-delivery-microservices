using ECommerce.Services.Identity.Shared.Models;

namespace ECommerce.Services.Identity.Users.Features.UpdatingUserState;

public record UpdateUserStateRequest
{
    public UserState UserState { get; init; }
}
