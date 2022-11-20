using ECommerce.Services.Identity.Identity.Dtos;
using ECommerce.Services.Identity.Identity.Dtos.v1;

namespace ECommerce.Services.Identity.Identity.Features.GeneratingRefreshToken.v1;

public record GenerateRefreshTokenResponse(RefreshTokenDto RefreshToken);
