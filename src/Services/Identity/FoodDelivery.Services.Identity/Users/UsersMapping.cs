using AutoMapper;
using FoodDelivery.Services.Identity.Shared.Models;
using FoodDelivery.Services.Identity.Users.Dtos.v1;
using FoodDelivery.Services.Identity.Users.Features.RegisteringUser.v1;

namespace FoodDelivery.Services.Identity.Users;

public class UsersMapping : Profile
{
    public UsersMapping()
    {
        CreateMap<RegisterUserRequest, RegisterUser>();

        CreateMap<ApplicationUser, IdentityUserDto>()
            .ForMember(x => x.RefreshTokens, opt => opt.MapFrom(x => x.RefreshTokens.Select(r => r.Token)))
            .ForMember(
                x => x.Roles,
                opt => opt.MapFrom(x => x.UserRoles.Where(m => m.Role != null).Select(q => q.Role!.Name))
            );
    }
}
