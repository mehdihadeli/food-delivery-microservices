using ECommerce.Services.Identity.Users.Dtos;
using ECommerce.Services.Identity.Users.Dtos.v1;

namespace ECommerce.Services.Identity.Users.Features.GettingUserById.v1;

internal record UserByIdResponse(IdentityUserDto IdentityUser);
