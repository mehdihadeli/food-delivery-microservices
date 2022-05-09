using Store.Services.Identity.Shared.Models;

namespace Store.Services.Identity.Users.Features.UpdatingUserState;

public record UpdateUserStateRequest
{
    public UserState UserState { get; init; }
}
