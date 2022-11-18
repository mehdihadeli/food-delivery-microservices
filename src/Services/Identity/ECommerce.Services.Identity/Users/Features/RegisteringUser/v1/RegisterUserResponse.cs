using ECommerce.Services.Identity.Users.Dtos;

namespace ECommerce.Services.Identity.Users.Features.RegisteringUser.v1;

internal record RegisterUserResponse(IdentityUserDto? UserIdentity);
