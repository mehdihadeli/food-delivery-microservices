using AutoMapper;
using ECommerce.Services.Identity.Shared.Models;
using ECommerce.Services.Identity.Users.Dtos;
using ECommerce.Services.Identity.Users.Dtos.v1;

namespace ECommerce.Services.Identity.Users;

public class UsersMapping : Profile
{
    public UsersMapping()
    {
        CreateMap<ApplicationUser, IdentityUserDto>()
            .ForMember(x => x.RefreshTokens, opt => opt.MapFrom(x => x.RefreshTokens.Select(r => r.Token)))
            .ForMember(
                x => x.Roles,
                opt => opt.MapFrom(x =>
                    x.UserRoles.Where(m => m.Role != null)
                        .Select(q => q.Role!.Name)));
    }
}
