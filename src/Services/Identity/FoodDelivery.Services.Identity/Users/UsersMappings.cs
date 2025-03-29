using FoodDelivery.Services.Identity.Users.Features.RegisteringUser.v1;
using Riok.Mapperly.Abstractions;

namespace FoodDelivery.Services.Identity.Users;

[Mapper]
public static partial class UsersMappings
{
    [MapperIgnoreTarget(nameof(RegisterUser.CreatedAt))]
    internal static partial RegisterUser ToRegisterUser(this RegisterUserRequest customerCreated);
}
