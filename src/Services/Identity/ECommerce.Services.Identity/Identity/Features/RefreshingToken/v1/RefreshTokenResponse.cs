using ECommerce.Services.Identity.Shared.Models;

namespace ECommerce.Services.Identity.Identity.Features.RefreshingToken.v1;

public record RefreshTokenResponse
{
    public RefreshTokenResponse(ApplicationUser user, string accessToken, string refreshToken)
    {
        UserId = user.Id;
        FirstName = user.FirstName;
        LastName = user.LastName;
        Username = user.UserName!;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
    }

    public string AccessToken { get; }
    public Guid UserId { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string Username { get; }
    public string RefreshToken { get; }
}
