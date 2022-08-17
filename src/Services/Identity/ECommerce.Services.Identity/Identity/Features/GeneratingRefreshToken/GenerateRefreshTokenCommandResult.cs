using ECommerce.Services.Identity.Identity.Dtos;

namespace ECommerce.Services.Identity.Identity.Features.GeneratingRefreshToken;

public record GenerateRefreshTokenCommandResult(RefreshTokenDto RefreshToken);
