using AutoMapper;
using Store.Services.Identity.Shared.Models;
using Store.Services.Identity.Users.Dtos;

namespace Store.Services.Identity.Users;

public class UsersMapping : Profile
{
    public UsersMapping()
    {
        CreateMap<ApplicationUser, IdentityUserDto>()
            .ForMember(x => x.RefreshTokens, opt => opt.MapFrom(x => x.RefreshTokens.Select(r => r.Token)));
    }
}
