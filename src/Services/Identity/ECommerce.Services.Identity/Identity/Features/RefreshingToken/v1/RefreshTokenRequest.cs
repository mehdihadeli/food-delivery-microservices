namespace ECommerce.Services.Identity.Identity.Features.RefreshingToken.v1;

public record RefreshTokenRequest(string AccessToken, string RefreshToken);
