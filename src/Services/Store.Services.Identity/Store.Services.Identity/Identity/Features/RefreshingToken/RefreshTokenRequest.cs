namespace Store.Services.Identity.Identity.Features.RefreshingToken;

public record RefreshTokenRequest(string AccessToken, string RefreshToken);
