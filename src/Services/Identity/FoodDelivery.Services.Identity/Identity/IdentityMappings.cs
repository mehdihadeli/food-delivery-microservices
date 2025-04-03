using FoodDelivery.Services.Identity.Identity.Features.Login.v1;
using FoodDelivery.Services.Identity.Identity.Features.RefreshingToken.v1;
using Riok.Mapperly.Abstractions;

namespace FoodDelivery.Services.Identity.Identity;

[Mapper]
public static partial class IdentityMappings
{
    internal static partial LoginResponse ToLoginResponse(this LoginResult loginResult);

    internal static partial RefreshTokenResponse ToRefreshTokenResponse(this RefreshTokenResult refreshTokenResult);
}
