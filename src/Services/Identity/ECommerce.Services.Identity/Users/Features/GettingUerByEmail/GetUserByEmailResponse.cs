using ECommerce.Services.Identity.Users.Dtos;

namespace ECommerce.Services.Identity.Users.Features.GettingUerByEmail;

public record GetUserByEmailResponse(IdentityUserDto? UserIdentity);
