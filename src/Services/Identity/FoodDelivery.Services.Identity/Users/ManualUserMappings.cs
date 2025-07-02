using FoodDelivery.Services.Identity.Shared.Models;
using FoodDelivery.Services.Identity.Users.Dtos.v1;

namespace FoodDelivery.Services.Identity.Users;

public static class ManualUserMappings
{
    internal static IdentityUserDto ToIdentityUserDto(this ApplicationUser applicationUser)
    {
        return new IdentityUserDto
        {
            Id = applicationUser.Id,
            Email = applicationUser.Email!,
            UserName = applicationUser.UserName!,
            FirstName = applicationUser.FirstName,
            LastName = applicationUser.LastName,
            PhoneNumber = applicationUser.PhoneNumber,
            UserState = applicationUser.UserState,
            CreatedAt = applicationUser.CreatedAt,
            LastLoggedInAt = applicationUser.LastLoggedInAt,
        };
    }

    internal static IQueryable<IdentityUserDto> ToIdentityUsersDto(this IQueryable<ApplicationUser> applicationUsers)
    {
        return applicationUsers.Select(applicationUser => new IdentityUserDto
        {
            Id = applicationUser.Id,
            Email = applicationUser.Email!,
            UserName = applicationUser.UserName!,
            FirstName = applicationUser.FirstName,
            LastName = applicationUser.LastName,
            PhoneNumber = applicationUser.PhoneNumber,
            UserState = applicationUser.UserState,
            CreatedAt = applicationUser.CreatedAt,
            LastLoggedInAt = applicationUser.LastLoggedInAt,
        });
    }
}
