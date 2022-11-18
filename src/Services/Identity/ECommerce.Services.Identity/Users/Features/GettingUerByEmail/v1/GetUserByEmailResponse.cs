using ECommerce.Services.Identity.Users.Dtos;

namespace ECommerce.Services.Identity.Users.Features.GettingUerByEmail.v1;

public record GetUserByEmailResponse(IdentityUserDto? UserIdentity);
