using ECommerce.Services.Identity.Users.Dtos;
using ECommerce.Services.Identity.Users.Dtos.v1;

namespace ECommerce.Services.Identity.Users.Features.GettingUerByEmail.v1;

public record GetUserByEmailResponse(IdentityUserDto? UserIdentity);
