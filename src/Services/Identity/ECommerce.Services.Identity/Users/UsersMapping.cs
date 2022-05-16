using AutoMapper;
using ECommerce.Services.Identity.Shared.Models;
using ECommerce.Services.Identity.Users.Dtos;

namespace ECommerce.Services.Identity.Users;

public class UsersMapping : Profile
{
    public UsersMapping()
    {
        CreateMap<ApplicationUser, IdentityUserDto>()
            .ForMember(x => x.RefreshTokens, opt => opt.MapFrom(x => x.RefreshTokens.Select(r => r.Token)));
    }
}
