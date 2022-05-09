using Store.Services.Identity.Identity.Dtos;

namespace Store.Services.Identity.Identity.Features.GenerateRefreshToken;

public record GenerateRefreshTokenCommandResult(RefreshTokenDto RefreshToken);
