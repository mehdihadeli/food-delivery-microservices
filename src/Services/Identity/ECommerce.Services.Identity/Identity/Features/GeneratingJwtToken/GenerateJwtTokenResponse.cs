namespace ECommerce.Services.Identity.Identity.Features.GeneratingJwtToken;

public record GenerateJwtTokenResponse(string Token, DateTime ExpireAt);
