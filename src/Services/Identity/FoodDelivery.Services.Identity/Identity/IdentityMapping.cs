using AutoMapper;
using FoodDelivery.Services.Identity.Identity.Features.Login.v1;
using FoodDelivery.Services.Identity.Identity.Features.RefreshingToken.v1;

namespace FoodDelivery.Services.Identity.Identity;

public class IdentityMapping : Profile
{
    public IdentityMapping()
    {
        CreateMap<LoginResult, LoginResponse>();
        CreateMap<RefreshTokenResult, RefreshTokenResponse>();
    }
}
