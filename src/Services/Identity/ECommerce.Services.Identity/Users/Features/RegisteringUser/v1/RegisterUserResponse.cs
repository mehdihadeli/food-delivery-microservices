using ECommerce.Services.Identity.Users.Dtos;
using ECommerce.Services.Identity.Users.Dtos.v1;

namespace ECommerce.Services.Identity.Users.Features.RegisteringUser.v1;

internal record RegisterUserResponse(IdentityUserDto? UserIdentity);
