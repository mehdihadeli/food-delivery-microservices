using ECommerce.Services.Identity.Identity.Dtos;

namespace ECommerce.Services.Identity.Identity.Features.GenerateRefreshToken;

public record GenerateRefreshTokenCommandResult(RefreshTokenDto RefreshToken);
