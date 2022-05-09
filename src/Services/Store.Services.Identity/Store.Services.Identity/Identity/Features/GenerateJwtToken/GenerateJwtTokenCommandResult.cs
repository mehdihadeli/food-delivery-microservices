using Microsoft.IdentityModel.JsonWebTokens;

namespace Store.Services.Identity.Identity.Features.GenerateJwtToken;

public record GenerateJwtTokenCommandResult(JsonWebToken JsonWebToken);
